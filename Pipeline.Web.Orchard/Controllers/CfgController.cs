﻿#region license
// Transformalize
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Notify;
using Transformalize.Contracts;
using Pipeline.Web.Orchard.Models;
using Pipeline.Web.Orchard.Services;
using System.IO;
using Orchard.Autoroute.Services;
using Orchard.FileSystems.AppData;
using Orchard.Services;
using Transformalize.Extensions;
using Process = Transformalize.Configuration.Process;

namespace Pipeline.Web.Orchard.Controllers {

    [ValidateInput(false), Themed(true)]
    public class CfgController : Controller {

        const string FileTimestamp = "yyyy-MM-dd-HH-mm-ss";
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private static readonly HashSet<string> _renderedOutputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "map", "page" };

        private readonly IOrchardServices _orchardServices;
        private readonly IProcessService _processService;
        private readonly ISortService _sortService;
        private readonly ISecureFileService _secureFileService;
        private readonly ICfgService _cfgService;
        private readonly ISlugService _slugService;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IClock _clock;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public CfgController(
            IOrchardServices services,
            IProcessService processService,
            ISortService sortService,
            ISecureFileService secureFileService,
            ICfgService cfgService,
            ISlugService slugService,
            IAppDataFolder appDataFolder,
            IClock clock
        ) {
            _clock = clock;
            _appDataFolder = appDataFolder;
            _orchardServices = services;
            _processService = processService;
            _secureFileService = secureFileService;
            _cfgService = cfgService;
            _sortService = sortService;
            _slugService = slugService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ActionResult List(string tagFilter) {

            // Sticky Tag Filter
            if (Request.RawUrl.EndsWith("List") || Request.RawUrl.Contains("List?")) {
                tagFilter = Session[Common.TagFilterName] != null ? Session[Common.TagFilterName].ToString() : Common.AllTag;
            } else {
                Session[Common.TagFilterName] = tagFilter;
            }

            if (!User.Identity.IsAuthenticated)
                System.Web.Security.FormsAuthentication.RedirectToLoginPage(Request.RawUrl);

            var viewModel = new ConfigurationListViewModel(
                _cfgService.List(tagFilter),
                Common.Tags<PipelineConfigurationPart, PipelineConfigurationPartRecord>(_orchardServices),
                tagFilter
            );

            return View(viewModel);
        }

        public ActionResult Report(int id) {

            var timer = new Stopwatch();
            timer.Start();

            var process = new Process { Name = "Report" };

            var part = _orchardServices.ContentManager.Get(id).As<PipelineConfigurationPart>();
            if (part == null) {
                process.Name = "Not Found";
            } else {

                var user = _orchardServices.WorkContext.CurrentUser == null ?
                    "Anonymous" :
                    _orchardServices.WorkContext.CurrentUser.UserName ?? "Anonymous";

                if (_orchardServices.Authorizer.Authorize(Permissions.ViewContent, part)) {

                    process = _processService.Resolve(part.EditorMode, part.EditorMode);

                    var parameters = Common.GetParameters(Request, _secureFileService, _orchardServices);
                    if (part.NeedsInputFile && Convert.ToInt32(parameters[Common.InputFileIdName]) == 0) {
                        _orchardServices.Notifier.Add(NotifyType.Error, T("This transformalize expects a file."));
                        process.Name = "File Not Found";
                    }

                    process.Load(part.Configuration, parameters);
                    process.Buffer = false; // no buffering for reports
                    process.ReadOnly = true;  // force reporting to omit system fields

                    var output = process.Output();

                    if (output.Provider.In("internal", "file")) {

                        Common.TranslatePageParametersToEntities(process, parameters, "page");

                        // change process for export purposes
                        var reportType = Request["output"] ?? "page";
                        if (!_renderedOutputs.Contains(reportType)) {
                            ConvertToExport(user, process, part, reportType, parameters);
                            process.Load(process.Serialize(), parameters);
                        }

                        if (Request["sort"] != null) {
                            _sortService.AddSortToEntity(process.Entities.First(), Request["sort"]);
                        }

                        if (process.Errors().Any()) {
                            foreach (var error in process.Errors()) {
                                _orchardServices.Notifier.Add(NotifyType.Error, T(error));
                            }
                        } else {
                            if (process.Entities.Any(e => !e.Fields.Any(f => f.Input))) {
                                _orchardServices.WorkContext.Resolve<ISchemaHelper>().Help(process);
                            }

                            if (!process.Errors().Any()) {

                                var runner = _orchardServices.WorkContext.Resolve<IRunTimeExecute>();
                                try {

                                    runner.Execute(process);
                                    process.Request = "Run";
                                    process.Time = timer.ElapsedMilliseconds;

                                    if (process.Errors().Any()) {
                                        foreach (var error in process.Errors()) {
                                            _orchardServices.Notifier.Add(NotifyType.Error, T(error));
                                        }
                                        process.Status = 500;
                                        process.Message = "There are errors in the pipeline.  See log.";
                                    } else {
                                        process.Status = 200;
                                        process.Message = "Ok";
                                    }

                                    var o = process.Output();
                                    switch (o.Provider) {
                                        case "kml":
                                        case "geojson":
                                        case "file":
                                            Response.AddHeader("content-disposition", "attachment; filename=" + o.File);
                                            switch (o.Provider) {
                                                case "kml":
                                                    Response.ContentType = "application/vnd.google-earth.kml+xml";
                                                    break;
                                                case "geojson":
                                                    Response.ContentType = "application/vnd.geo+json";
                                                    break;
                                                default:
                                                    Response.ContentType = "application/csv";
                                                    break;
                                            }
                                            Response.Flush();
                                            Response.End();
                                            return new EmptyResult();
                                        case "excel":
                                            return new FilePathResult(o.File, ExcelContentType) {
                                                FileDownloadName = _slugService.Slugify(part.Title()) + ".xlsx"
                                            };
                                        default:  // page and map
                                            break;
                                    }
                                } catch (Exception ex) {
                                    Logger.Error(ex, ex.Message);
                                    _orchardServices.Notifier.Error(T(ex.Message));
                                }
                            }
                        }
                    }
                } else {
                    _orchardServices.Notifier.Warning(user == "Anonymous" ? T("Sorry. Anonymous users do not have permission to view this report. You may need to login.") : T("Sorry {0}. You do not have permission to view this report.", user));
                }
            }

            return View(new ReportViewModel(process, part));

        }

        private void ConvertToExport(string user, Process process, PipelineConfigurationPart part, string exportType, IDictionary<string, string> parameters) {
            var o = process.Output();
            switch (exportType) {
                case "xlsx":
                    if (!_appDataFolder.DirectoryExists(Common.FileFolder)) {
                        _appDataFolder.CreateDirectory(Common.FileFolder);
                    }

                    var fileName = $"{user}-{_clock.UtcNow.ToString(FileTimestamp)}-{_slugService.Slugify(part.Title())}.xlsx";

                    o.Provider = "excel";
                    o.File = _appDataFolder.MapPath(_appDataFolder.Combine(Common.FileFolder, fileName));
                    break;
                case "geojson":
                    o.Stream = true;
                    o.Provider = "geojson";
                    o.File = _slugService.Slugify(part.Title()) + ".geojson";
                    break;
                case "kml":
                    o.Stream = true;
                    o.Provider = "kml";
                    o.File = _slugService.Slugify(part.Title()) + ".kml";
                    break;
                default: //csv
                    o.Stream = true;
                    o.Provider = "file";
                    o.Delimiter = ",";
                    o.File = _slugService.Slugify(part.Title()) + ".csv";
                    break;
            }

            parameters["page"] = "0";
            foreach (var entity in process.Entities) {
                entity.Page = 0;
                foreach (var field in entity.GetAllFields().Where(f => !f.System)) {
                    field.T = string.Empty;
                    if (field.Output && field.Transforms.Any()) {
                        var lastTransform = field.Transforms.Last();
                        if (lastTransform.Method == "tag" || lastTransform.Method == "razor" && field.Raw) {
                            var firstParameter = lastTransform.Parameters.First();
                            field.Transforms.Remove(lastTransform);
                            field.T = "copy(" + firstParameter.Field + ")";
                        }
                    }
                }
                entity.Fields.RemoveAll(f => f.System);
            }
        }

        [Themed(false)]
        [HttpGet]
        public ActionResult Download(int id) {

            var part = _orchardServices.ContentManager.Get(id).As<PipelineConfigurationPart>();

            var process = new Process { Name = "Export" };

            if (part == null) {
                process.Name = "Not Found";
                return new FileStreamResult(GenerateStreamFromString(process.Serialize()), "text/xml") { FileDownloadName = id + ".xml" };
            }

            if (!_orchardServices.Authorizer.Authorize(Permissions.ViewContent, part)) {
                process.Name = "Not Authorized";
                return new FileStreamResult(GenerateStreamFromString(process.Serialize()), "text/xml") { FileDownloadName = id + ".xml" };
            }

            return new FileStreamResult(GenerateStreamFromString(part.Configuration), "text/" + part.EditorMode) { FileDownloadName = _slugService.Slugify(part.Title()) + "." + part.EditorMode };

        }


        public Stream GenerateStreamFromString(string s) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


    }
}