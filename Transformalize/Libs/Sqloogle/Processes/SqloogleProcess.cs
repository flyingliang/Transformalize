﻿/*
   Copyright 2013 Dale Newman

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Transformalize.Libs.Rhino.Etl.Operations;
using Transformalize.Libs.Sqloogle.Operations;
using Transformalize.Main;
using Transformalize.Main.Providers;

namespace Transformalize.Libs.Sqloogle.Processes {

    /// <summary>
    /// The SQLoogle process.  This is where it all comes together.  It SQLoogles!
    /// </summary>
    public class SqloogleProcess : PartialProcessOperation {

        public SqloogleProcess(Process process, AbstractConnection connection)
            : base(process) {
            Register(new ServerCrawlProcess(process, connection));
            Register(new SqloogleAggregate());
            Register(new SqloogleTransform());
        }

    }
}
