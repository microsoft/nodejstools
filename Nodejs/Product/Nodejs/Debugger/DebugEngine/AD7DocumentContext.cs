//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    /// <summary>
    /// This class represents a document context to the debugger. A document context
    /// represents a location within a source file.
    /// </summary>
    internal class AD7DocumentContext : IDebugDocumentContext2
    {
        private readonly AD7MemoryAddress _codeContext;

        public AD7DocumentContext(AD7MemoryAddress codeContext)
        {
            _codeContext = codeContext;
        }

        public AD7Engine Engine
        {
            get { return _codeContext.Engine; }
        }

        public NodeModule Module
        {
            get { return _codeContext.Module; }
        }

        public string FileName
        {
            get { return _codeContext.FileName; }
        }

        public bool Downloaded
        {
            get
            {
                // No directory separator characters implies downloaded
                return (FileName.IndexOf(Path.DirectorySeparatorChar) == -1);
            }
        }

        #region IDebugDocumentContext2 Members

        // Compares this document context to a given array of document contexts.
        int IDebugDocumentContext2.Compare(enum_DOCCONTEXT_COMPARE compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            pdwDocContext = 0;

            return VSConstants.E_NOTIMPL;
        }

        // Retrieves a list of all code contexts associated with this document context.
        // The engine sample only supports one code context per document context and 
        // the code contexts are always memory addresses.
        int IDebugDocumentContext2.EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            var codeContexts = new AD7MemoryAddress[1];
            codeContexts[0] = _codeContext;
            ppEnumCodeCxts = new AD7CodeContextEnum(codeContexts);
            return VSConstants.S_OK;
        }

        // Gets the document that contains this document context.
        // This method is for those debug engines that supply documents directly to the IDE. Since the sample engine
        // does not do this, this method returns E_NOTIMPL.
        int IDebugDocumentContext2.GetDocument(out IDebugDocument2 ppDocument)
        {
            // Expose document for downloaded modules
            ppDocument = null;
            if (Downloaded)
            {
                NodeModule module = Module;
                if (module != null)
                {
                    // Lazily create document per module
                    ppDocument = (AD7Document)module.Document;
                    if (ppDocument == null)
                    {
                        ppDocument = new AD7Document(this);
                        module.Document = ppDocument;
                    }
                }
            }
            return ppDocument != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        // Gets the language associated with this document context.
        // The language for this sample is always C++
        int IDebugDocumentContext2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            AD7Engine.MapLanguageInfo(FileName, out pbstrLanguage, out pguidLanguage);
            return VSConstants.S_OK;
        }

        // Gets the displayable name of the document that contains this document context.
        int IDebugDocumentContext2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            pbstrFileName = FileName;
            return pbstrFileName != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        // Gets the source code range of this document context.
        // A source range is the entire range of source code, from the current statement back to just after the previous s
        // statement that contributed code. The source range is typically used for mixing source statements, including 
        // comments, with code in the disassembly window.
        // Sincethis engine does not support the disassembly window, this is not implemented.
        int IDebugDocumentContext2.GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            throw new NotImplementedException("This method is not implemented");
        }

        // Gets the file statement range of the document context.
        // A statement range is the range of the lines that contributed the code to which this document context refers.
        int IDebugDocumentContext2.GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            pBegPosition[0].dwColumn = (uint)_codeContext.Column;
            pBegPosition[0].dwLine = (uint)_codeContext.Line;

            pEndPosition[0].dwColumn = (uint)_codeContext.Column;
            pEndPosition[0].dwLine = (uint)_codeContext.Line;

            return VSConstants.S_OK;
        }

        // Moves the document context by a given number of statements or lines.
        // This is used primarily to support the Autos window in discovering the proximity statements around 
        // this document context. 
        int IDebugDocumentContext2.Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}