﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Transformalize.Libs.Newtonsoft.Json;
using Transformalize.Main;
using Transformalize.Orchard.Models;

namespace Transformalize.Orchard.Handlers {
    public static class JsonContentHandler {
        private const string JSON_TEMPLATE = @"{{
    ""request"":""{0}"",
    ""status"":{1},
    ""message"":""{2}"",
    ""time"":{3},
    ""environments"":{4},
    ""processes"":{5},
    ""response"":{6},
    ""log"":{7}
}}";

        public static string LogsToJson(IEnumerable<string> logs) {
            var sw = new StringWriter();
            var writer = new JsonTextWriter(sw);
            writer.WriteStartArray();
            foreach (var log in logs) {
                writer.WriteStartObject();

                var attributes = log.Split(new []{" | "}, 5, StringSplitOptions.None);
                writer.WritePropertyName("time");
                writer.WriteValue(attributes[0]);
                writer.WritePropertyName("level");
                writer.WriteValue(attributes[1].TrimEnd());
                writer.WritePropertyName("process");
                writer.WriteValue(attributes[2]);
                writer.WritePropertyName("entity");
                writer.WriteValue(attributes[3]);
                writer.WritePropertyName("message");
                writer.WriteValue(attributes[4].TrimEnd(new []{' ','\r','\n'}));

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.Flush();
            return sw.ToString();
        }

        public static string GetContent(ApiRequest request, string configuration, TransformalizeResponse response, string meta) {

            var builder = new StringBuilder();

            if (request.Status != 200) {
                return ApiContentHandler.GetErrorContent(JSON_TEMPLATE, request);
            }

            var converter = new OneWayXmlNodeConverter();

            XElement doc;
            string processes;

            switch (request.RequestType) {
                case ApiRequestType.MetaData:
                    var metaData = JsonConvert.SerializeObject(XDocument.Parse(meta).Descendants("entities").First(), Formatting.None, converter);
                    builder.AppendFormat(JSON_TEMPLATE, "metadata", 200, "OK", request.Stopwatch.ElapsedMilliseconds, string.Empty, string.Empty, metaData, LogsToJson(response.Log));
                    return builder.ToString();

                case ApiRequestType.Configuration:
                    doc = XDocument.Parse(configuration).Root;
                    string environments = JsonConvert.SerializeObject(doc.Elements("environments").Any() ? doc.Element("environments").Nodes() : new string[0] as object, Formatting.None, converter);
                    processes = JsonConvert.SerializeObject(doc.Element("processes").Nodes(), Formatting.None, converter);
                    builder.AppendFormat(JSON_TEMPLATE, "configuration", 200, "OK", request.Stopwatch.ElapsedMilliseconds, environments, processes, "[]", LogsToJson(response.Log));
                    return builder.ToString();

                case ApiRequestType.Execute:
                    string results;
                    doc = XDocument.Parse(configuration).Root;
                    var nodes = doc.Element("processes");
                    nodes.Descendants("parameters").Remove();
                    nodes.Descendants("connections").Remove();
                    processes = JsonConvert.SerializeObject(nodes.Nodes(), Formatting.None, converter);
                    switch (request.Flavor) {
                        case "arrays":
                            results = new JsonResultsToArrayHandler().Handle(response.Processes);
                            break;
                        case "array":
                            goto case "arrays";
                        case "dictionaries":
                            results = new JsonResultsToDictionaryHandler().Handle(response.Processes);
                            break;
                        case "dictionary":
                            goto case "dictionaries";
                        default:
                            results = new JsonResultsToObjectHandler().Handle(response.Processes);
                            break;
                    }
                    builder.AppendFormat(JSON_TEMPLATE, "execute", 200, "OK", request.Stopwatch.ElapsedMilliseconds, "[]", processes, results, LogsToJson(response.Log));
                    return builder.ToString();

                default:
                    builder.AppendFormat(JSON_TEMPLATE, "configuration", 200, "OK", request.Stopwatch.ElapsedMilliseconds, "[]", "[]", "[]", LogsToJson(response.Log));
                    return builder.ToString();
            }
        }

    }
}