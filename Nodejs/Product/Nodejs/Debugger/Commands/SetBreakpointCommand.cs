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
    sealed class SetBreakpointCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;
        private readonly NodeBreakpoint _breakpoint;
        private readonly NodeModule _module;

        public SetBreakpointCommand(int id, NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate = false)
            : base(id, "setbreakpoint") {
            _module = module;
            _breakpoint = breakpoint;

            // Zero based line numbers
            int line = breakpoint.LineNo - 1;

            // Zero based column numbers
            // Special case column to avoid (line 0, column 0) which
            // Node (V8) treats specially for script loaded via require
            int column = line == 0 ? 1 : 0;

            _arguments = new Dictionary<string, object> {
                { "line", line },
                { "column", column }
            };

            if (_module != null) {
                _arguments["type"] = "scriptId";
                _arguments["target"] = _module.ModuleId;
            } else {
                _arguments["type"] = "scriptRegExp";
                _arguments["target"] = GetCaseInsensitiveRegex(_breakpoint.FileName);
            }

            if (!NodeBreakpointBinding.GetEngineEnabled(_breakpoint.Enabled, _breakpoint.BreakOn, 0)) {
                _arguments["enabled"] = false;
            }

            if (withoutPredicate) {
                return;
            }

            int ignoreCount = NodeBreakpointBinding.GetEngineIgnoreCount(_breakpoint.BreakOn, 0);
            if (ignoreCount > 0) {
                _arguments["ignoreCount"] = ignoreCount;
            }

            if (!string.IsNullOrEmpty(_breakpoint.Condition)) {
                _arguments["condition"] = _breakpoint.Condition;
            }
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public int BreakpointId { get; private set; }

        public int? ScriptId { get; private set; }

        public int LineNo { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            if (_module != null) {
                ScriptId = _module.ModuleId;
            }

            JToken body = response["body"];
            BreakpointId = (int)body["breakpoint"];

            // Handle breakpoint actual location fixup
            LineNo = _breakpoint.LineNo;

            var actualLocations = (JArray)body["actual_locations"];
            if (actualLocations == null) {
                return;
            }

            if (actualLocations.Count > 0) {
                int actualLocation = (int)actualLocations[0]["line"] + 1;
                if (actualLocation != _breakpoint.LineNo) {
                    LineNo = actualLocation;
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