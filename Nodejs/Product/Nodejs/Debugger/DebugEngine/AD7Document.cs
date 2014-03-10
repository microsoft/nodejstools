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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {

    // Private interop to use IntPtr for potentially null out parameters
    [Guid("4B0645AA-08EF-4CB9-ADB9-0395D6EDAD35")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugDocumentText2_Private {
        [PreserveSig]
        int GetDocumentClassId(out Guid pclsid);
        [PreserveSig]
        int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName);
        [PreserveSig]
        int GetSize(IntPtr pcNumLines, IntPtr pcNumChars);
        [PreserveSig]
        int GetText(TEXT_POSITION pos, uint cMaxChars, IntPtr pText, IntPtr pcNumChars);
    }

    class AD7Document : IDebugDocument2, IDebugDocumentText2_Private {
        private readonly AD7DocumentContext _documentContext;
        private char[] _scriptText;
        private int[] _scriptLines;

        public AD7Document(AD7DocumentContext documentContext) {
            _documentContext = documentContext;
        }

        private char[] ScriptText {
            get {
                if (_scriptText == null) {
                    var moduleId = _documentContext.Module.Id;
                    var scriptText = _documentContext.Engine.Process.GetScriptTextAsync(moduleId).Result;
                    if (scriptText != null){
                        _scriptText = scriptText.ToCharArray();
                    }
                }

                return _scriptText;
            }
        }

        private int[] ScriptLines {
            get {
                if (_scriptLines == null) {
                    var scriptLines = new List<int> { 0 };
                    if (ScriptText != null) {
                        for (var i = 0; i < ScriptText.Length; ++i) {
                            // Treat combinations of carriage return and line feed as line endings
                            var curChar = ScriptText[i];
                            if (curChar == '\r' || curChar == '\n') {
                                if (curChar == '\r' && i + 1 < ScriptText.Length) {
                                    if (ScriptText[i + 1] == '\n') {
                                        ++i;
                                    }
                                }
                                scriptLines.Add(i + 1);
                            }
                        }
                    }
                    _scriptLines = scriptLines.ToArray();
                }
                return _scriptLines;
            }
        }

        private int CharPosFromTextPos(TEXT_POSITION textPosition) {
            // Lazily calculate ScriptLines
            if (textPosition.dwLine == 0) {
                return (int)textPosition.dwColumn;
            }

            var scriptLines = ScriptLines;
            var line = textPosition.dwLine < scriptLines.Length ? (int)textPosition.dwLine : scriptLines.Length - 1;
            return scriptLines[line] + (int)textPosition.dwColumn;
        }

        private static void SetOutParameterValue(IntPtr outParameter, Func<Int32> valueFunc) {
            // Avoid evaluating and setting out parameter value if null
            if (outParameter != IntPtr.Zero) {
                Marshal.Copy(new[] { valueFunc() }, 0, outParameter, 1);
            }
        }

        #region IDebugDocument2 Members

        public int GetDocumentClassId(out Guid pclsid) {
            pclsid = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName) {
            return ((IDebugDocumentContext2)_documentContext).GetName(gnType, out pbstrFileName);
        }

        #endregion

        #region IDebugDocumentText2 Members

        public int GetSize(IntPtr pcNumLines, IntPtr pcNumChars) {
            SetOutParameterValue(pcNumLines, () => ScriptLines.Length);
            SetOutParameterValue(pcNumChars, () => ScriptText != null ? ScriptText.Length : 0);
            return VSConstants.S_OK;
        }

        public int GetText(TEXT_POSITION pos, uint cMaxChars, IntPtr pText, IntPtr pcNumChars) {
            SetOutParameterValue(pcNumChars, () => 0);
            if (pText == IntPtr.Zero) {
                return VSConstants.E_INVALIDARG;
            }
            if (ScriptText != null) {
                var charPos = CharPosFromTextPos(pos);
                var availChars = ScriptText.Length > charPos ? ScriptText.Length - charPos : 0;
                var cNumChars = Math.Min((int)cMaxChars, availChars);
                Marshal.Copy(ScriptText, 0, pText, cNumChars);
                SetOutParameterValue(pcNumChars, () => cNumChars);
            }
            return VSConstants.S_OK;
        }

        #endregion
    }
}
