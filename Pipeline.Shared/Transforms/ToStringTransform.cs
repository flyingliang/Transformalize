﻿#region license
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
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms {
    public class ToStringTransform : BaseTransform {
        readonly Field _input;
        readonly Func<object, string> _toString;

        public ToStringTransform(IContext context) : base(context, "string") {
            _input = SingleInput();
            if (context.Transform.Format == string.Empty) {
                _toString = (o) => o.ToString();
            } else {
                switch (_input.Type) {
                    case "int32":
                    case "int":
                        _toString = (o) => ((int)o).ToString(context.Transform.Format);
                        break;
                    case "double":
                        _toString = (o) => ((double)o).ToString(context.Transform.Format);
                        break;
                    case "short":
                    case "int16":
                        _toString = (o) => ((short)o).ToString(context.Transform.Format);
                        break;
                    case "long":
                    case "int64":
                        _toString = (o) => ((long)o).ToString(context.Transform.Format);
                        break;
                    case "datetime":
                    case "date":
                        _toString = (o) => ((DateTime)o).ToString(context.Transform.Format);
                        break;
                    default:
                        _toString = (o) => o.ToString();
                        break;
                }
            }
        }

        public override IRow Transform(IRow row) {
            row[Context.Field] = _toString(row[_input]);
            Increment();
            return row;
        }
    }
}