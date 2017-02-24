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
using System.Globalization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    // And implementation of IDebugCodeContext2 and IDebugMemoryContext2. 
    // IDebugMemoryContext2 represents a position in the address space of the machine running the program being debugged.
    // IDebugCodeContext2 represents the starting position of a code instruction. 
    // For most run-time architectures today, a code context can be thought of as an address in a program's execution stream.
    internal sealed class AD7MemoryAddress : IDebugCodeContext2, IDebugCodeContext100
    {
        private readonly int _column;
        private readonly AD7Engine _engine;
        private readonly string _fileName;
        private readonly NodeStackFrame _frame;
        private readonly int _line;
        private IDebugDocumentContext2 _documentContext;

        private AD7MemoryAddress(AD7Engine engine, NodeStackFrame frame, string fileName, int line, int column)
        {
            _engine = engine;
            _frame = frame;
            _fileName = fileName;
            _line = line;
            _column = column;
        }

        public AD7MemoryAddress(AD7Engine engine, string fileName, int line, int column)
            : this(engine, null, fileName, line, column)
        {
        }

        public AD7MemoryAddress(AD7Engine engine, NodeStackFrame frame)
            : this(engine, frame, null, frame.Line, frame.Column)
        {
        }

        public AD7Engine Engine
        {
            get { return _engine; }
        }

        public NodeModule Module
        {
            get { return _frame != null ? _frame.Module : null; }
        }

        public string FileName
        {
            get { return _frame != null ? _frame.FileName : _fileName; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int Column
        {
            get { return _column; }
        }

        public void SetDocumentContext(IDebugDocumentContext2 docContext)
        {
            _documentContext = docContext;
        }

        #region IDebugMemoryContext2 Members

        // Adds a specified value to the current context's address to create a new context.
        public int Add(ulong dwCount, out IDebugMemoryContext2 newAddress)
        {
            newAddress = new AD7MemoryAddress(_engine, _frame, _fileName, _line + (int)dwCount, _column);
            return VSConstants.S_OK;
        }

        // Compares the memory context to each context in the given array in the manner indicated by compare flags, 
        // returning an index of the first context that matches.
        public int Compare(enum_CONTEXT_COMPARE uContextCompare, IDebugMemoryContext2[] compareToItems, uint compareToLength, out uint foundIndex)
        {
            foundIndex = uint.MaxValue;

            enum_CONTEXT_COMPARE contextCompare = uContextCompare;

            for (uint c = 0; c < compareToLength; c++)
            {
                var compareTo = compareToItems[c] as AD7MemoryAddress;
                if (compareTo == null)
                {
                    continue;
                }

                if (!ReferenceEquals(_engine, compareTo._engine))
                {
                    continue;
                }

                bool result;

                switch (contextCompare)
                {
                    case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                        result = _line == compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                        result = _line < compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                        result = _line > compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                        result = _line <= compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                        result = _line >= compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                        if (_frame != null)
                        {
                            result = compareTo.FileName == FileName && compareTo._line >= _frame.StartLine && compareTo._line <= _frame.EndLine;
                        }
                        else if (compareTo._frame != null)
                        {
                            result = compareTo.FileName == FileName && _line >= compareTo._frame.StartLine && compareTo._line <= compareTo._frame.EndLine;
                        }
                        else
                        {
                            result = _line == compareTo._line && FileName == compareTo.FileName;
                        }
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                        result = FileName == compareTo.FileName;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                        result = true;
                        break;

                    default:
                        // A new comparison was invented that we don't support
                        return VSConstants.E_NOTIMPL;
                }

                if (result)
                {
                    foundIndex = c;
                    return VSConstants.S_OK;
                }
            }

            return VSConstants.S_FALSE;
        }

        // Gets information that describes this context.
        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            pinfo[0].dwFields = 0;

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0)
            {
                pinfo[0].bstrAddress = _line.ToString(CultureInfo.InvariantCulture);
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0 && _frame != null)
            {
                pinfo[0].bstrFunction = _frame.FunctionName;
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION;
            }

            // Fields not supported by the sample
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET) != 0)
            {
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0)
            {
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0)
            {
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET) != 0)
            {
            }

            return VSConstants.S_OK;
        }

        // Gets the user-displayable name for this context
        // This is not supported by the sample engine.
        public int GetName(out string pbstrName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Subtracts a specified value from the current context's address to create a new context.
        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(_engine, _frame, _fileName, _line - (int)dwCount, _column);
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugCodeContext2 Members

        // Gets the document context for this code-context
        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = _documentContext;
            return VSConstants.S_OK;
        }

        // Gets the language information for this code context.
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            if (_documentContext != null)
            {
                return _documentContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);
            }
            AD7Engine.MapLanguageInfo(FileName, out pbstrLanguage, out pguidLanguage);
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugCodeContext100 Members

        // Returns the program being debugged. In the case of this sample
        // debug engine, AD7Engine implements IDebugProgram2 which represents
        // the program being debugged.
        int IDebugCodeContext100.GetProgram(out IDebugProgram2 pProgram)
        {
            pProgram = _engine;
            return VSConstants.S_OK;
        }

        #endregion
    }
}