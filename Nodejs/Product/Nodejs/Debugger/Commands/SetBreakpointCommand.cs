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
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class SetBreakpointCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;
        private readonly NodeBreakpoint _breakpoint;
        private readonly NodeModule _module;
        private readonly SourceMapper _sourceMapper;
        private readonly FilePosition _position;

        public SetBreakpointCommand(int id, NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate, bool remote, SourceMapper sourceMapper = null)
            : base(id, "setbreakpoint") {
            Utilities.ArgumentNotNull("breakpoint", breakpoint);

            _module = module;
            _breakpoint = breakpoint;
            _sourceMapper = sourceMapper;

            _position = breakpoint.GetPosition(_sourceMapper);

            // Zero based line numbers
            int line = _position.Line;

            // Zero based column numbers
            // Special case column to avoid (line 0, column 0) which
            // Node (V8) treats specially for script loaded via require
            // Script wrapping process: https://github.com/joyent/node/blob/v0.10.26-release/src/node.js#L880
            int column = _position.Column;
            if (line == 0) {
                column += NodeConstants.ScriptWrapBegin.Length;
            }

            _arguments = new Dictionary<string, object> {
                { "line", line },
                { "column", column }
            };

            if (_module != null) {
                _arguments["type"] = "scriptId";
                _arguments["target"] = _module.Id;
            } else if (remote) {
                _arguments["type"] = "scriptRegExp";
                _arguments["target"] = GetCaseInsensitiveRegex(_position.FileName);
            } else {
                _arguments["type"] = "script";
                _arguments["target"] = _position.FileName;
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

        public int Line { get; private set; }

        public int Column { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];
            BreakpointId = (int)body["breakpoint"];

            int? moduleId = _module != null ? _module.Id : (int?)null;
            ScriptId = (int?)body["script_id"] ?? moduleId;

            // Handle breakpoint actual location fixup
            JArray actualLocations = (JArray)body["actual_locations"] ?? new JArray();
            if (actualLocations.Count > 0) {
                var location = actualLocations[0];
                ScriptId = ScriptId ?? (int?)location["script_id"];
                Line = (int)location["line"];
                var column = (int)location["column"];
                Column = Line == 0 ? column - NodeConstants.ScriptWrapBegin.Length : column;
            } else {
                Line = _position.Line;
                Column = _position.Column;
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