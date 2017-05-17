// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
            this._engine = engine;
            this._frame = frame;
            this._fileName = fileName;
            this._line = line;
            this._column = column;
        }

        public AD7MemoryAddress(AD7Engine engine, string fileName, int line, int column)
            : this(engine, null, fileName, line, column)
        {
        }

        public AD7MemoryAddress(AD7Engine engine, NodeStackFrame frame)
            : this(engine, frame, null, frame.Line, frame.Column)
        {
        }

        public AD7Engine Engine => this._engine;
        public NodeModule Module => this._frame != null ? this._frame.Module : null;
        public string FileName => this._frame != null ? this._frame.FileName : this._fileName;
        public int Line => this._line;
        public int Column => this._column;
        public void SetDocumentContext(IDebugDocumentContext2 docContext)
        {
            this._documentContext = docContext;
        }

        #region IDebugMemoryContext2 Members

        // Adds a specified value to the current context's address to create a new context.
        public int Add(ulong dwCount, out IDebugMemoryContext2 newAddress)
        {
            newAddress = new AD7MemoryAddress(this._engine, this._frame, this._fileName, this._line + (int)dwCount, this._column);
            return VSConstants.S_OK;
        }

        // Compares the memory context to each context in the given array in the manner indicated by compare flags, 
        // returning an index of the first context that matches.
        public int Compare(enum_CONTEXT_COMPARE uContextCompare, IDebugMemoryContext2[] compareToItems, uint compareToLength, out uint foundIndex)
        {
            foundIndex = uint.MaxValue;

            var contextCompare = uContextCompare;

            for (uint c = 0; c < compareToLength; c++)
            {
                var compareTo = compareToItems[c] as AD7MemoryAddress;
                if (compareTo == null)
                {
                    continue;
                }

                if (!ReferenceEquals(this._engine, compareTo._engine))
                {
                    continue;
                }

                bool result;

                switch (contextCompare)
                {
                    case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                        result = this._line == compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                        result = this._line < compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                        result = this._line > compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                        result = this._line <= compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                        result = this._line >= compareTo._line;
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                        if (this._frame != null)
                        {
                            result = compareTo.FileName == this.FileName && compareTo._line >= this._frame.StartLine && compareTo._line <= this._frame.EndLine;
                        }
                        else if (compareTo._frame != null)
                        {
                            result = compareTo.FileName == this.FileName && this._line >= compareTo._frame.StartLine && compareTo._line <= compareTo._frame.EndLine;
                        }
                        else
                        {
                            result = this._line == compareTo._line && this.FileName == compareTo.FileName;
                        }
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                        result = this.FileName == compareTo.FileName;
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
                pinfo[0].bstrAddress = this._line.ToString(CultureInfo.InvariantCulture);
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0 && this._frame != null)
            {
                pinfo[0].bstrFunction = this._frame.FunctionName;
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
            ppMemCxt = new AD7MemoryAddress(this._engine, this._frame, this._fileName, this._line - (int)dwCount, this._column);
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugCodeContext2 Members

        // Gets the document context for this code-context
        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = this._documentContext;
            return VSConstants.S_OK;
        }

        // Gets the language information for this code context.
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            if (this._documentContext != null)
            {
                return this._documentContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);
            }
            AD7Engine.MapLanguageInfo(this.FileName, out pbstrLanguage, out pguidLanguage);
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugCodeContext100 Members

        // Returns the program being debugged. In the case of this sample
        // debug engine, AD7Engine implements IDebugProgram2 which represents
        // the program being debugged.
        int IDebugCodeContext100.GetProgram(out IDebugProgram2 pProgram)
        {
            pProgram = this._engine;
            return VSConstants.S_OK;
        }

        #endregion
    }
}

