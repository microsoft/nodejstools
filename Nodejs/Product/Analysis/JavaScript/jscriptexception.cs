// jscriptexception.cs
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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Microsoft.Ajax.Utilities;

namespace Microsoft.NodejsTools.Parsing
{

    //-------------------------------------------------------------------------------------------------------
    // JScriptException
    //
    //  An error in JScript goes to a COM+ host/program in the form of a JScriptException. However a 
    //  JScriptException is not always thrown. In fact a JScriptException is also a IVsaError and thus it can be
    //  passed to the host through IVsaSite.OnCompilerError(IVsaError error).
    //  When a JScriptException is not a wrapper for some other object (usually either a COM+ exception or 
    //  any value thrown in a JScript throw statement) it takes a JSError value.
    //  The JSError enum is defined in JSError.cs. When introducing a new type of error perform
    //  the following four steps:
    //  1- Add the error in the JSError enum (JSError.cs)
    //  2- Update JScript.resx with the US English error message
    //  3- Update Severity.
    //-------------------------------------------------------------------------------------------------------
#if !SILVERLIGHT
    [Serializable]
#endif
    public class JScriptException : Exception
    {
        #region private fields

#if !SILVERLIGHT
        [NonSerialized]
#endif

        private string m_valueObject;
        private bool m_isError;
        private JSError m_errorCode;
        private readonly IndexSpan _span;
        private readonly LocationResolver _resolver;
        private bool m_canRecover = true;

        #endregion

        #region constructors

        public JScriptException(string message) : this(message, null) { }
        public JScriptException(string message, Exception innerException)
            : base(message, innerException) {
            m_valueObject = message;
            m_errorCode = JSError.UncaughtException;
            SetHResult();
        }

        internal JScriptException(JSError errorNumber, IndexSpan span, LocationResolver resolver) {
            m_valueObject = null;
            _span = span;
            _resolver = resolver;
            m_errorCode = errorNumber;
            SetHResult();
        }

#if !SILVERLIGHT
        protected JScriptException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            if (info == null) {
                throw new ArgumentException(JScript.InternalCompilerError);
            }

            m_valueObject = info.GetString("Value");
            m_isError = info.GetBoolean("IsError");
            m_errorCode = (JSError)info.GetInt32("JSError");
            m_canRecover = info.GetBoolean("CanRecover");
        }
#endif

        #endregion

        #region public properties

        public IndexSpan Span {
            get {
                return _span;
            }
        }

        public bool CanRecover
        {
            get { return m_canRecover; }
            set { m_canRecover = value; }
        }

        public bool IsError
        {
            get { return m_isError; }
            set { m_isError = value; }
        }

        public string Value
        {
            get { return m_valueObject; }
            set { m_valueObject = value; }
        }

        public int StartColumn
        {
            get
            {
                return Column;
            }
        }

        public int Line
        {
            get
            {
                if (_resolver != null)
                {
                    return _resolver.IndexToLocation(_span.Start).Line;
                }
                else
                {
                    return 1;
                }
            }
        }

        public int Column
        {
            get
            {
                if (_resolver != null)
                {
                    // one-based column number
                    return _resolver.IndexToLocation(_span.Start).Column;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int EndLine
        {
            get
            {
                if (_resolver != null)
                {
                    return _resolver.IndexToLocation(_span.End).Line;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int EndColumn
        {
            get
            {
                if (_resolver != null)
                {
                    return _resolver.IndexToLocation(_span.End).Column;
                }
                else
                    return 0;
            }
        }

        public override String Message
        {
            get
            {
                string code = m_errorCode.ToString();
                if (m_valueObject != null)
                {
                    return (m_errorCode == JSError.DuplicateName)
                        ? JScript.ResourceManager.GetString(code, JScript.Culture).FormatInvariant(m_valueObject)
                        : m_valueObject;
                }

                // special case some errors with contextual information
                return JScript.ResourceManager.GetString(code, JScript.Culture).FormatInvariant(
                    "");    // TODO: Re-enable getting context code?
#if FALSE
                    m_context.IfNotNull(c => c.HasCode) ? string.Empty : m_context.Code);
#endif
            }
        }

        public JSError ErrorCode
        {
            get { return m_errorCode; }
        }

        public int Severity
        {
            get
            {
                return GetSeverity(m_errorCode);
            }
        }

        #endregion
        

        #region public methods

        #if !SILVERLIGHT
        [SecurityCritical] 
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
            info.AddValue("Value", m_valueObject);
            info.AddValue("IsError", m_isError);
            info.AddValue("JSError", (int)m_errorCode);
            info.AddValue("CanRecover", m_canRecover);
        }
        #endif

        #endregion

        #region public static methods

        /// <summary>
        /// Return the default severity for a given JSError value
        /// guide: 0 == there will be a run-time error if this code executes
        ///        1 == the programmer probably did not intend to do this
        ///        2 == this can lead to cross-browser of future problems.
        ///        3 == this can lead to performance problems
        ///        4 == this is just not right
        /// </summary>
        /// <param name="errorCode">error code</param>
        /// <returns>severity</returns>
        public static int GetSeverity(JSError errorCode)
        {
            switch (errorCode)
            {
                case JSError.AmbiguousCatchVar:
                case JSError.AmbiguousNamedFunctionExpression:
                case JSError.NumericOverflow:
                case JSError.StrictComparisonIsAlwaysTrueOrFalse:
                    return 1;

                case JSError.DuplicateCatch:
                case JSError.DuplicateConstantDeclaration:
                case JSError.DuplicateLexicalDeclaration:
                case JSError.KeywordUsedAsIdentifier:
                case JSError.MisplacedFunctionDeclaration:
                case JSError.ObjectLiteralKeyword:
                    return 2;

                case JSError.ArgumentNotReferenced:
                case JSError.DuplicateName:
                case JSError.FunctionNotReferenced:
                case JSError.UndeclaredFunction:
                case JSError.UndeclaredVariable:
                case JSError.VariableDefinedNotReferenced:
                    return 3;

                case JSError.StatementBlockExpected:
                case JSError.SuspectAssignment:
                case JSError.SuspectSemicolon:
                case JSError.SuspectEquality:
                case JSError.WithNotRecommended:
                case JSError.ObjectConstructorTakesNoArguments:
                case JSError.NumericMaximum:
                case JSError.NumericMinimum:
                case JSError.OctalLiteralsDeprecated:
                case JSError.FunctionNameMustBeIdentifier:
                case JSError.SemicolonInsertion:
                    return 4;

                default:
                    // all others
                    return 0;
            }
        }

        #endregion

        #region private methods

        private void SetHResult()
        {
            this.HResult = //unchecked((int)(0x800A0000 + (int)m_errorCode));
                -2146828288 + (int)m_errorCode;
        }

        #endregion
    }

    public class JScriptExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The JavaScript error information being fired
        /// </summary>
        public ContextError Error { get; private set; }

        /// <summary>
        /// JScriptException object. Don't use this; might go away in future versions. Use Error property instead.
        /// </summary>
        public JScriptException Exception { get; private set; }

        public JScriptExceptionEventArgs(JScriptException exception, ContextError error)
        {
            Error = error;
            Exception = exception;
        }
    }
}
