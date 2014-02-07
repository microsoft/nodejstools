/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class SetBreakpointCommand : DebuggerCommandBase {
        private readonly NodeBreakpoint _breakpoint;
        private readonly NodeModule _module;

        public SetBreakpointCommand(int id, int line, int column, NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate = false) : base(id) {
            _module = module;
            _breakpoint = breakpoint;

            CommandName = "setbreakpoint";
            Arguments = new Dictionary<string, object> {
                { "line", line },
                { "column", column }
            };

            if (_module != null) {
                Arguments["type"] = "scriptId";
                Arguments["target"] = _module.ModuleId;
            } else {
                Arguments["type"] = "scriptRegExp";
                Arguments["target"] = GetCaseInsensitiveRegex(_breakpoint.FileName);
            }

            if (!NodeBreakpointBinding.GetEngineEnabled(_breakpoint.Enabled, _breakpoint.BreakOn, 0)) {
                Arguments["enabled"] = false;
            }

            if (!withoutPredicate) {
                int ignoreCount = NodeBreakpointBinding.GetEngineIgnoreCount(_breakpoint.BreakOn, 0);
                if (ignoreCount > 0) {
                    Arguments["ignoreCount"] = ignoreCount;
                }

                if (!string.IsNullOrEmpty(_breakpoint.Condition)) {
                    Arguments["condition"] = _breakpoint.Condition;
                }
            }
        }

        public int BreakpointId { get; private set; }

        public int? ScriptId { get; private set; }

        public int LineNo { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];
            BreakpointId = (int)body["breakpoint"];
            if (_module != null) {
                ScriptId = _module.ModuleId;
            }

            // Handle breakpoint actual location fixup
            LineNo = _breakpoint.LineNo;
            var actualLocations = (JArray)body["actual_locations"];
            if (actualLocations != null) {
                if (actualLocations.Count > 0) {
                    int actualLocation = (int)actualLocations[0]["line"] + 1;
                    if (actualLocation != _breakpoint.LineNo) {
                        LineNo = actualLocation;
                    }
                }
            }
        }

        private string GetCaseInsensitiveRegex(string filePath) {
            // NOTE: There is no way to pass a regex case insensitive modifier to the Node (V8) engine
            string fileName = Path.GetFileName(filePath) ?? string.Empty;
            bool trailing = fileName != filePath;

            fileName = Regex.Escape(fileName);

            var builder = new StringBuilder();
            if (trailing) {
                string separators = string.Format("{0}{1}", Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                builder.Append("[" + Regex.Escape(separators) + "]");
            } else {
                builder.Append('^');
            }

            foreach (char ch in fileName) {
                string upper = ch.ToString(CultureInfo.InvariantCulture).ToUpper();
                string lower = ch.ToString(CultureInfo.InvariantCulture).ToLower();
                if (upper != lower) {
                    builder.Append('[');
                    builder.Append(upper);
                    builder.Append(lower);
                    builder.Append(']');
                } else {
                    builder.Append(upper);
                }
            }

            builder.Append("$");
            return builder.ToString();
        }
    }
}