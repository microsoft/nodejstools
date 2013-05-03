// documentcontext.cs
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

using System;
using System.Collections.Generic;

namespace Microsoft.Ajax.Utilities
{
    public class DocumentContext
    {
        private Dictionary<string, string> m_reportedVariables;

        private JSParser m_parser;

        public string Source { get; private set; }
        public string FileContext { get; set; }
        public bool IsGenerated { get; private set; }

        public DocumentContext(JSParser parser)
        {
            m_parser = parser;
            IsGenerated = true;
            Source = "[generated code]";
        }

        public DocumentContext(JSParser parser, string source)
        {
            m_parser = parser;
            Source = source;
        }

        internal DocumentContext DifferentFileContext(string fileContext)
        {
            // use the SAME parser and reported variable dictionary
            var documentContext = new DocumentContext(m_parser, Source)
            {
                IsGenerated = this.IsGenerated,
                FileContext = fileContext
            };

            documentContext.m_reportedVariables = this.m_reportedVariables;
            return documentContext;
        }

        //---------------------------------------------------------------------------------------
        // HandleError
        //
        //  Handle an error. There are two actions that can be taken when an error occurs:
        //    - throwing an exception with no recovering action (eval case)
        //    - notify the host that an error occurred and let the host decide whether or not
        //      parsing has to continue (the host returns true when parsing has to continue)
        //---------------------------------------------------------------------------------------

        internal void HandleError(JScriptException error)
        {
            if (m_parser != null)
            {
                if (!m_parser.OnCompilerError(error))
                {
                    throw new EndOfFileException(); // this exception terminates the parser
                }
            }
        }

        internal void ReportUndefined(UndefinedReferenceException ex)
        {
            if (m_parser != null)
            {
                m_parser.OnUndefinedReference(ex);
            }
        }

        internal bool HasAlreadySeenErrorFor(String varName)
        {
            if (m_reportedVariables == null)
            {
                m_reportedVariables = new Dictionary<string, string>();
            }
            else if (m_reportedVariables.ContainsKey(varName))
            {
                return true;
            }
            m_reportedVariables.Add(varName, varName);
            return false;
        }

    }
}
