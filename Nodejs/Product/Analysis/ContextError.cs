// ContextError.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text;

namespace Microsoft.NodejsTools.Parsing {
    internal class ContextError
    {
        // error information properties
        public bool IsError { get; protected set; }
        public int Severity { get; protected set; }
        public string Subcategory { get; protected set; }
        public JSError ErrorCode { get; protected set; }
        public string HelpKeyword { get; protected set; }
        public string File { get; protected set; }
        public int StartLine { get; protected set; }
        public int StartColumn { get; protected set; }
        public int EndLine { get; protected set; }
        public int EndColumn { get; protected set; }
        public string Message { get; protected set; }

        // constructor
        public ContextError(bool isError, int severity, string subcategory, JSError errorCode, string helpKeyword, int startLine, int startColumn, int endLine, int endColumn, string message)
        {
            // transfer the values as-is
            IsError = isError;
            Severity = severity;
            Subcategory = subcategory;
            ErrorCode = errorCode;
            HelpKeyword = helpKeyword;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            Message = message;
        }

        /// <summary>
        /// Convert the exception to a VisualStudio format error message
        /// file(startline[-endline]?,startcol[-endcol]?):[ subcategory] category [errorcode]: message
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            // if there is a startline, then there must be a location.
            // no start line, then no location
            if (StartLine > 0)
            {
                // we will always at least start with the start line
                sb.AppendFormat("({0}", StartLine);

                if (EndLine > StartLine)
                {
                    if (StartColumn > 0 && EndColumn > 0)
                    {
                        // all four values were specified
                        sb.AppendFormat(",{0},{1},{2}", StartColumn, EndLine, EndColumn);
                    }
                    else
                    {
                        // one or both of the columns wasn't specified, so ignore them both
                        sb.AppendFormat("-{0}", EndLine);
                    }
                }
                else if (StartColumn > 0)
                {
                    sb.AppendFormat(",{0}", StartColumn);
                    if (EndColumn > StartColumn)
                    {
                        sb.AppendFormat("-{0}", EndColumn);
                    }
                }

                sb.Append(')');
            }

            // seaprate the location from the error description
            sb.Append(':');

            // if there is a subcategory, add it prefaced with a space
            if (!string.IsNullOrEmpty(Subcategory))
            {
                sb.Append(' ');
                sb.Append(Subcategory);
            }

            // not localizable
            sb.Append(IsError ? " error " : " warning ");

            // if there is an error code
            
            sb.Append("JS{0}".FormatInvariant((int)ErrorCode));

            // separate description from the message
            sb.Append(": ");

            if (!string.IsNullOrEmpty(Message))
            {
                sb.Append(Message);
            }

            return sb.ToString();
        }
    }
}
