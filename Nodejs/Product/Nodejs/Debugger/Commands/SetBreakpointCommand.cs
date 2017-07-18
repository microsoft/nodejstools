// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class SetBreakpointCommand : DebuggerCommand
    {
        private static string _pathSeperatorCharacterGroup = string.Format(CultureInfo.InvariantCulture, "[{0}{1}]", Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private readonly Dictionary<string, object> _arguments;
        private readonly NodeBreakpoint _breakpoint;
        private readonly NodeModule _module;
        private readonly SourceMapper _sourceMapper;
        private readonly FilePosition _position;

        public SetBreakpointCommand(int id, NodeModule module, NodeBreakpoint breakpoint, bool withoutPredicate, bool remote, SourceMapper sourceMapper = null)
            : base(id, "setbreakpoint")
        {
            Utilities.ArgumentNotNull("breakpoint", breakpoint);

            this._module = module;
            this._breakpoint = breakpoint;
            this._sourceMapper = sourceMapper;

            this._position = breakpoint.GetPosition(this._sourceMapper);

            // Zero based line numbers
            var line = this._position.Line;

            // Zero based column numbers
            // Special case column to avoid (line 0, column 0) which
            // Node (V8) treats specially for script loaded via require
            // Script wrapping process: https://github.com/joyent/node/blob/v0.10.26-release/src/node.js#L880
            var column = this._position.Column;
            if (line == 0)
            {
                column += NodeConstants.ScriptWrapBegin.Length;
            }

            this._arguments = new Dictionary<string, object> {
                { "line", line },
                { "column", column }
            };

            if (this._module != null)
            {
                this._arguments["type"] = "scriptId";
                this._arguments["target"] = this._module.Id;
            }
            else if (remote)
            {
                this._arguments["type"] = "scriptRegExp";
                this._arguments["target"] = CreateRemoteScriptRegExp(this._position.FileName);
            }
            else
            {
                this._arguments["type"] = "scriptRegExp";
                this._arguments["target"] = CreateLocalScriptRegExp(this._position.FileName);
            }

            if (!NodeBreakpointBinding.GetEngineEnabled(this._breakpoint.Enabled, this._breakpoint.BreakOn, 0))
            {
                this._arguments["enabled"] = false;
            }

            if (withoutPredicate)
            {
                return;
            }

            var ignoreCount = NodeBreakpointBinding.GetEngineIgnoreCount(this._breakpoint.BreakOn, 0);
            if (ignoreCount > 0)
            {
                this._arguments["ignoreCount"] = ignoreCount;
            }

            if (!string.IsNullOrEmpty(this._breakpoint.Condition))
            {
                this._arguments["condition"] = this._breakpoint.Condition;
            }
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
        public int BreakpointId { get; private set; }

        public int? ScriptId { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            var body = response["body"];
            this.BreakpointId = (int)body["breakpoint"];

            var moduleId = this._module != null ? this._module.Id : (int?)null;
            this.ScriptId = (int?)body["script_id"] ?? moduleId;

            // Handle breakpoint actual location fixup
            var actualLocations = (JArray)body["actual_locations"] ?? new JArray();
            if (actualLocations.Count > 0)
            {
                var location = actualLocations[0];
                this.ScriptId = this.ScriptId ?? (int?)location["script_id"];
                this.Line = (int)location["line"];
                var column = (int)location["column"];
                this.Column = this.Line == 0 ? column - NodeConstants.ScriptWrapBegin.Length : column;
            }
            else
            {
                this.Line = this._position.Line;
                this.Column = this._position.Column;
            }
        }

        private static string CreateRemoteScriptRegExp(string filePath)
        {
            var fileName = Path.GetFileName(filePath) ?? string.Empty;
            var start = fileName == filePath ? "^" : _pathSeperatorCharacterGroup;
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}$", start, CreateCaseInsensitiveRegExpFromString(fileName));
        }

        public static string CreateLocalScriptRegExp(string filePath)
        {
            return string.Format(CultureInfo.InvariantCulture, "^{0}$", CreateCaseInsensitiveRegExpFromString(filePath));
        }

        /// <summary>
        /// Convert a string into a case-insensitive regular expression.
        /// 
        /// This is a workaround for the fact that we cannot pass a regex case insensitive modifier to the Node (V8) engine.
        /// </summary>
        private static string CreateCaseInsensitiveRegExpFromString(string str)
        {
            var builder = new StringBuilder();
            foreach (var ch in Regex.Escape(str))
            {
                var upper = ch.ToString(CultureInfo.InvariantCulture).ToUpper();
                var lower = ch.ToString(CultureInfo.InvariantCulture).ToLower();
                if (upper != lower)
                {
                    builder.Append('[');
                    builder.Append(upper);
                    builder.Append(lower);
                    builder.Append(']');
                }
                else
                {
                    builder.Append(upper);
                }
            }
            return builder.ToString();
        }
    }
}
