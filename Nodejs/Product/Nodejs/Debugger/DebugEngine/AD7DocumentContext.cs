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
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {
    // This class represents a document context to the debugger. A document context represents a location within a source file. 
    class AD7DocumentContext : IDebugDocumentContext2 {
        private readonly AD7MemoryAddress _codeContext;
        private string _fileName;

        public AD7DocumentContext(AD7MemoryAddress codeContext) {
            _codeContext = codeContext;
        }

        public AD7Engine Engine {
            get {
                return _codeContext.Engine;
            }
        }

        public NodeModule Module {
            get {
                return _codeContext.Module;
            }
        }

        public string FileName {
            get {
                if (_fileName == null) {
                    // Perform "fuzzy" filename matching for non-builtin sources
                    var module = Module;
                    _fileName =
                        (module == null) || module.BuiltIn ?
                        _codeContext.FileName :
                        Engine.GetFuzzyMatchFilename(_codeContext.FileName);
                }
                return _fileName;
            }
        }

        public bool Downloaded {
            get {
                // No directory separator characters implies downloaded
                return (FileName.IndexOf(Path.DirectorySeparatorChar) == -1);
            }
        }

        #region IDebugDocumentContext2 Members

        // Compares this document context to a given array of document contexts.
        int IDebugDocumentContext2.Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext) {
            dwDocContextSetLen = 0;
            pdwDocContext = 0;

            return VSConstants.E_NOTIMPL;
        }

        // Retrieves a list of all code contexts associated with this document context.
        // The engine sample only supports one code context per document context and 
        // the code contexts are always memory addresses.
        int IDebugDocumentContext2.EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts) {
            ppEnumCodeCxts = null;

            AD7MemoryAddress[] codeContexts = new AD7MemoryAddress[1];
            codeContexts[0] = _codeContext;
            ppEnumCodeCxts = new AD7CodeContextEnum(codeContexts);
            return VSConstants.S_OK;
        }

        // Gets the document that contains this document context.
        // This method is for those debug engines that supply documents directly to the IDE. Since the sample engine
        // does not do this, this method returns E_NOTIMPL.
        int IDebugDocumentContext2.GetDocument(out IDebugDocument2 ppDocument) {
            // Expose document for downloaded modules
            ppDocument = null;
            if (Downloaded) {
                var module = Module;
                if (module != null) {
                    // Lazily create document per module
                    ppDocument = (AD7Document)module.Document;
                    if (ppDocument == null) {
                        ppDocument = new AD7Document(this);
                        module.Document = ppDocument;
                    }
                }
            }
            return ppDocument != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        // Gets the language associated with this document context.
        // The language for this sample is always C++
        int IDebugDocumentContext2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage) {
            pbstrLanguage = NodejsConstants.Nodejs;
            pguidLanguage = Guids.NodejsDebugLanguage;
            return VSConstants.S_OK;
        }

        // Gets the displayable name of the document that contains this document context.
        int IDebugDocumentContext2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName) {
            pbstrFileName = FileName;
            return pbstrFileName != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        // Gets the source code range of this document context.
        // A source range is the entire range of source code, from the current statement back to just after the previous s
        // statement that contributed code. The source range is typically used for mixing source statements, including 
        // comments, with code in the disassembly window.
        // Sincethis engine does not support the disassembly window, this is not implemented.
        int IDebugDocumentContext2.GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition) {
            throw new NotImplementedException("This method is not implemented");
        }

        // Gets the file statement range of the document context.
        // A statement range is the range of the lines that contributed the code to which this document context refers.
        int IDebugDocumentContext2.GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition) {
            pBegPosition[0].dwColumn = 0;
            pBegPosition[0].dwLine = _codeContext.LineNumber;

            pEndPosition[0].dwColumn = 0;
            pEndPosition[0].dwLine = _codeContext.LineNumber;

            return VSConstants.S_OK;
        }

        // Moves the document context by a given number of statements or lines.
        // This is used primarily to support the Autos window in discovering the proximity statements around 
        // this document context. 
        int IDebugDocumentContext2.Seek(int nCount, out IDebugDocumentContext2 ppDocContext) {
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
