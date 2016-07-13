#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
// Copyright 2013 Dale Newman
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
using Cfg.Net;
using Pipeline.Context;
using Pipeline.Contracts;

namespace Pipeline.Logging {
    public class LogEntry : CfgNode {

        public PipelineContext Context { get; }

        [Cfg]
        public DateTime Time { get; private set; }

        [Cfg(value = "info")]
        public string Level { get; set; }

        public LogLevel LogLevel { get; private set; }

        [Cfg(value = "")]
        public string Message { get; private set; }

        public Exception Exception { get; set; }

        public LogEntry(LogLevel level, PipelineContext context, string message, params object[] args) {
            Time = DateTime.UtcNow;
            Context = context;
            LogLevel = level;
            Message = string.Format(message, args);
        }

        public LogEntry() {
            Time = DateTime.UtcNow;
        }

        [Cfg]
        public string Process => Context?.Process.Name;

        [Cfg]
        public string Entity => Context?.Entity?.Name;

        [Cfg]
        public string Field => Context?.Field?.Alias;

        [Cfg]
        public string Transform => Context?.Transform?.Method;


    }
}