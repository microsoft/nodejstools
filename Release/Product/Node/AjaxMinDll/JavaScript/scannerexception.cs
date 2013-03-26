// scannerexception.cs
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
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Ajax.Utilities
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ScannerException : Exception
    {
        private JSError m_errorId;

        private static string s_syntaxErrorMsg = JScript.SyntaxError;

        internal ScannerException(JSError errorId)
            : base(s_syntaxErrorMsg)
        {
            m_errorId = errorId;
        }

        public ScannerException()
        {
            m_errorId = JSError.SyntaxError;
        }
        public ScannerException(string message)
            : base(message)
        {
            m_errorId = JSError.SyntaxError;
        }

        public ScannerException(string message, Exception innerException)
            : base(message, innerException)
        {
            m_errorId = JSError.SyntaxError;
        }

#if !SILVERLIGHT
        protected ScannerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            m_errorId = (JSError)Enum.Parse(typeof(JSError), info.GetString("errorid"));
        }

        [SecurityCritical]
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            info.AddValue("errorId", m_errorId.ToString());
        }
#endif

        public JSError Error
        {
            get { return m_errorId; }
        }
    }
}

