#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections.Generic;
using Cfg.Net.Contracts;
using Dapper;
using Transformalize.Actions;
using Transformalize.Contracts;
using Transformalize.Extensions;

namespace Transformalize.Provider.Ado.Actions {
    public class AdoRunAction : IAction {
        private readonly IContext _context;
        private readonly Configuration.Action _node;
        private readonly IConnectionFactory _cf;
        private readonly IReader _commandReader;

        public AdoRunAction(IContext context, Configuration.Action node, IConnectionFactory cf, IReader commandReader) {
            _context = context;
            _node = node;
            _cf = cf;
            _commandReader = commandReader;
        }

        public ActionResponse Execute() {
            var response = new ActionResponse();
            using (var cn = _cf.GetConnection()) {
                cn.Open();
                try {
                    if (_node.Command == string.Empty) {
                        var logger = new Cfg.Net.Loggers.MemoryLogger();
                        _node.Command = _commandReader.Read(_node.Url == string.Empty ? _node.File : _node.Url, new Dictionary<string, string>(), logger);
                        foreach (var warning in logger.Warnings()) {
                            _context.Warn(warning);
                        }
                        foreach (var error in logger.Errors()) {
                            _context.Error(error);
                        }
                    }
                    _node.RowCount = cn.Execute(_node.Command, commandTimeout: _node.TimeOut);
                    var message = $"{(_node.Description == string.Empty ? _node.Type + " action" : "'" + _node.Description + "'")} affected {(_node.RowCount == -1 ? 0 : _node.RowCount)} row{_node.RowCount.Plural()}.";
                    response.Message = message;
                    _context.Info(message);
                } catch (Exception ex) {
                    response.Code = 500;
                    response.Message = ex.Message + " " + ex.StackTrace + " " + _node.Command.Replace("{", "{{").Replace("}", "}}");
                }
            }
            return response;
        }
    }
}
