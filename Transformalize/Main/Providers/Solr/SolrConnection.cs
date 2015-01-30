using System;
using System.Collections.Generic;
using Transformalize.Configuration;
using Transformalize.Libs.Ninject.Syntax;
using Transformalize.Libs.Rhino.Etl.Operations;
using Transformalize.Libs.SolrNet;

namespace Transformalize.Main.Providers.Solr {

    public class SolrConnection : AbstractConnection {
        private const int DEFAULT_PORT = 8983;
        private readonly char[] _slash = { '/' };
        private readonly string _solrUrl;
        private readonly string _schemaFile = "schema.xml";

        public static Dictionary<string, string> SolrTypeMap = new Dictionary<string, string>() {
            {"text_en_splitting","string"}
        };

        public SolrConnection(TflConnection element, AbstractConnectionDependencies dependencies)
            : base(element, dependencies) {
            Type = ProviderType.Solr;
            IsDatabase = true;
            TextQualifier = string.Empty;
            _solrUrl = element.NormalizeUrl(DEFAULT_PORT);

            if (!element.File.Equals(string.Empty)) {
                _schemaFile = element.File;
            }

        }

        public string GetPingUrl() {
            return _solrUrl.TrimEnd(_slash) + "/admin/ping";
        }

        public string GetCoreUrl(string coreName) {
            return _solrUrl.TrimEnd(_slash) + "/" + coreName.TrimStart(_slash);
        }

        public override int NextBatchId(string processName) {
            if (!TflBatchRecordsExist(processName)) {
                return 1;
            }
            return GetMaxTflBatchId(processName) + 1;
        }

        public override void WriteEndVersion(Process process, AbstractConnection input, Entity entity, bool force = false) {

            if (entity.Updates + entity.Inserts > 0 || force) {

                var solr = GetOperations(process, entity.OutputName());
                var versionType = entity.Version == null ? "string" : entity.Version.SimpleType;

                string end;
                if (versionType.Equals("datetime") && entity.End is DateTime) {
                    end = ((DateTime)entity.End).ToString("yyyy-MM-ddTHH:mm:ss.fff");
                } else if (versionType.Equals("byte[]") || versionType.Equals("rowversion")) {
                    end = Common.BytesToHexString((byte[])entity.End);
                } else {
                    end = new DefaultFactory().Convert(entity.End, versionType).ToString();
                }

                var doc = new Dictionary<string, object> {
                    { "id", entity.TflBatchId},
                    { "tflbatchid", entity.TflBatchId},
                    { "process", entity.ProcessName},
                    { "connection", input.Name},
                    { "entity", entity.Alias},
                    { "updates", entity.Updates},
                    { "inserts", entity.Inserts},
                    { "deletes", entity.Deletes},
                    { "version", end},
                    { "version_type", versionType},
                    { "tflupdate", DateTime.UtcNow}
                };
                solr.Add(doc);
                solr.Commit();
            }
        }

        public override IOperation ExtractCorrespondingKeysFromOutput(Entity entity) {
            return new SolrEntityOutputKeysExtract(this, entity);
        }

        public override IOperation ExtractAllKeysFromOutput(Entity entity) {
            return new SolrEntityOutputKeysExtract(this, entity);
        }

        public override IOperation ExtractAllKeysFromInput(Process process, Entity entity) {
            return new SolrExtract(process, this, entity.PrimaryKey, entity.OutputName());
        }

        public override IOperation Insert(Process process, Entity entity) {
            return new SolrLoadOperation(entity, this);
        }

        public override IOperation Update(Entity entity) {
            return new SolrLoadOperation(entity, this);
        }

        public override void LoadBeginVersion(Entity entity) {
            var tflBatchId = GetMaxTflBatchId(entity);
            if (tflBatchId > 0) {
                //entity.Begin = Common.GetObjectConversionMap()[versionType](hits[0]["_source"]["version"].Value);
                entity.HasRange = true;
            }
        }

        public override void LoadEndVersion(Entity entity) {

            var body = new {
                aggs = new {
                    version = new {
                        max = new {
                            field = entity.Version.Alias.ToLower()
                        }
                    }
                },
                size = 0
            };
            //var result = client.Client.Search(client.Index, client.Type, body);
            //entity.End = Common.GetObjectConversionMap()[entity.Version.SimpleType](result.Response["aggregations"]["version"]["value"].Value);
            entity.HasRows = entity.End != null;
        }

        public override Fields GetEntitySchema(Process process, Entity entity, bool isMaster = false) {
            var fields = new Fields();
            var solr = GetReadonlyOperations(process, entity.OutputName());
            var solrSchema = solr.GetSchema(_schemaFile);

            foreach (var solrField in solrSchema.SolrFields) {
                string type;
                var searchType = "default";
                if (SolrTypeMap.ContainsKey(solrField.Type.Name)) {
                    type = SolrTypeMap[solrField.Type.Name];
                    searchType = solrField.Type.Name;
                } else {
                    type = solrField.Type.Name;
                }

                var field = new Field(type, "64", FieldType.None, true, string.Empty) {
                    Name = solrField.Name,
                    Entity = entity.Name,
                    Input = solrField.IsStored,
                    SearchTypes = new List<SearchType>() { new SearchType() { Name = searchType, Analyzer = searchType } }
                };
                fields.Add(field);
            }
            return fields;
        }

        public override IOperation Delete(Entity entity) {
            throw new NotImplementedException();
        }

        public override IOperation Extract(Process process, Entity entity, bool firstRun) {
            return new SolrExtract(process, this, entity.Fields.WithInput(), entity.OutputName());
        }

        private int GetMaxTflBatchId(Entity entity) {
            var body = new {
                query = new {
                    query_string = new {
                        query = entity.Alias,
                        fields = new[] { "entity" }
                    }
                },
                aggs = new {
                    tflbatchid = new {
                        max = new {
                            field = "tflbatchid"
                        }
                    }
                },
                size = 0
            };
            //var result = client.Client.Search(client.Index, client.Type, body);
            //return Convert.ToInt32((result.Response["aggregations"]["tflbatchid"]["value"].Value ?? 0));
            throw new NotImplementedException();
        }

        private int GetMaxTflBatchId(string processName) {

            var body = new {
                aggs = new {
                    tflbatchid = new {
                        max = new {
                            field = "tflbatchid"
                        }
                    }
                },
                size = 0
            };
            //var result = client.Client.Search(client.Index, client.Type, body);
            //return Convert.ToInt32((result.Response["aggregations"]["tflbatchid"]["value"].Value ?? 0));
            throw new NotImplementedException();
        }

        public ISolrReadOnlyOperations<Dictionary<string, object>> GetReadonlyOperations(Process process, string outputName) {
            var coreUrl = GetCoreUrl(outputName);
            return process.Kernal.Get<ISolrReadOnlyOperations<Dictionary<string, object>>>(coreUrl);
        }

        public ISolrOperations<Dictionary<string, object>> GetOperations(Process process, string outputName) {
            var coreUrl = GetCoreUrl(outputName);
            return process.Kernal.Get<ISolrOperations<Dictionary<string, object>>>(coreUrl);
        }
    }
}