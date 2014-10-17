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

using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    internal class NodejsSignature : ISignature, IOverloadResult {
        private readonly ITrackingSpan _span;
        private readonly string _content, _ppContent;
        private readonly string _documentation;
        private readonly ReadOnlyCollection<IParameter> _parameters;
        private IParameter _currentParameter;
        private readonly IOverloadResult _overload;

        public NodejsSignature(ITrackingSpan span, IOverloadResult overload, int paramIndex, string lastKeywordArg = null) {
            _span = span;
            _overload = overload;
            if (lastKeywordArg != null) {
                paramIndex = Int32.MaxValue;
            }

            string documentation = overload.Documentation;

            var content = new StringBuilder(overload.Name);
            var ppContent = new StringBuilder(overload.Name);
            content.Append('(');
            ppContent.AppendLine("(");
            int start = content.Length, ppStart = ppContent.Length;
            var parameters = new IParameter[overload.Parameters.Length];
            for (int i = 0; i < overload.Parameters.Length; i++) {
                ppContent.Append("    ");
                ppStart = ppContent.Length;

                var param = overload.Parameters[i];
                if (i > 0) {
                    content.Append(", ");
                    start = content.Length;
                }

                // Try to locate and parse the corresponding @param in the doclet.
                string docRegex =
                    // @param, @arg or @argument, and optional {type}
                    (@"@(param|arg|argument) (\s+|\s*\{(?<Type>[^}]*?)\}\s*)" + 
                    // Either just name by itself, or [name] if it's optional, or [name=value] if defaulted.
                     @"(?<IsOptional>\[\s*)? {{VariableName}} (?(IsOptional)\s*(=\s*(?<DefaultValue>.*?)\s*)?\])" +
                    // Associated docstring. Ends at the end of doclet, or at the next non-inline @tag, or at the next empty line (paragraph break).
                     @"\s* (?<Documentation>.*?) \s* ($|\r\n\r\n|(?<!\{)@)" 
                    ).Replace("{{VariableName}}", param.Name);
                var m = Regex.Match(documentation, docRegex, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

                content.Append(param.Name);
                ppContent.Append(param.Name);
                if (param.IsOptional || m.Groups["IsOptional"].Success) {
                    content.Append("?");
                    ppContent.Append("?");
                }

                string type = null;
                if (!string.IsNullOrEmpty(param.Type) && param.Type != "object") {
                    type = param.Type;
                } else if (m.Groups["Type"].Success) {
                    type = m.Groups["Type"].Value;
                }
                if (type != null) {
                    content.Append(": ");
                    content.Append(type);
                    ppContent.Append(": ");
                    ppContent.Append(type);
                }

                if (m.Groups["DefaultValue"].Success && m.Groups["DefaultValue"].Length > 0) {
                    content.Append(" = ");
                    content.Append(m.Groups["DefaultValue"].Value);
                }

                var paramSpan = new Span(start, content.Length - start);
                var ppParamSpan = new Span(ppStart, ppContent.Length - ppStart);

                ppContent.AppendLine(",");

                if (lastKeywordArg != null && param.Name == lastKeywordArg) {
                    paramIndex = i;
                }

                string paramDoc = null;
                if (m.Groups["Documentation"].Success) {
                    paramDoc = m.Groups["Documentation"].Value.Replace("\r\n", " ");
                }

                parameters[i] = new NodejsParameter(this, param, paramSpan, ppParamSpan, paramDoc);
            }
            content.Append(')');
            ppContent.Append(')');

            _content = content.ToString();
            _ppContent = ppContent.ToString();
            _documentation = overload.Documentation.LimitLines(15, stopAtFirstBlankLine: true);

            _parameters = new ReadOnlyCollection<IParameter>(parameters);
            if (paramIndex < parameters.Length) {
                _currentParameter = parameters[paramIndex];
            } else {
                _currentParameter = null;
            }
        }

        internal void SetCurrentParameter(IParameter newValue) {
            if (newValue != _currentParameter) {
                var args = new CurrentParameterChangedEventArgs(_currentParameter, newValue);
                _currentParameter = newValue;
                var changed = CurrentParameterChanged;
                if (changed != null) {
                    changed(this, args);
                }
            }
        }

        public ITrackingSpan ApplicableToSpan {
            get { return _span; }
        }

        public string Content {
            get { return _content; }
        }

        public IParameter CurrentParameter {
            get { return _currentParameter; }
        }

        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        public string Documentation {
            get { return _documentation; }
        }

        public ReadOnlyCollection<IParameter> Parameters {
            get { return _parameters; }
        }

        #region ISignature Members


        public string PrettyPrintedContent {
            get { return _ppContent; }
        }

        #endregion

        string IOverloadResult.Name {
            get { return _overload.Name; }
        }

        string IOverloadResult.Documentation {
            get { return _documentation; }
        }

        ParameterResult[] IOverloadResult.Parameters {
            get { return _overload.Parameters; }
        }
    }
}
