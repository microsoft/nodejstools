// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockOutputWindowPane : IVsOutputWindowPane
    {
        private string _name;
        private readonly StringBuilder _content = new StringBuilder();

        public MockOutputWindowPane(string pszPaneName)
        {
            _name = pszPaneName;
        }

        public int Activate()
        {
            return VSConstants.S_OK;
        }

        public int Clear()
        {
            _content.Clear();
            return VSConstants.S_OK;
        }

        public int FlushToTaskList()
        {
            throw new NotImplementedException();
        }

        public int GetName(ref string pbstrPaneName)
        {
            pbstrPaneName = _name;
            return VSConstants.S_OK;
        }

        public int Hide()
        {
            return VSConstants.S_OK;
        }

        public int OutputString(string pszOutputString)
        {
            lock (this)
            {
                _content.Append(pszOutputString);
            }
            return VSConstants.S_OK;
        }

        public int OutputStringThreadSafe(string pszOutputString)
        {
            lock (this)
            {
                _content.Append(pszOutputString);
            }
            return VSConstants.S_OK;
        }

        public int OutputTaskItemString(string pszOutputString, VSTASKPRIORITY nPriority, VSTASKCATEGORY nCategory, string pszSubcategory, int nBitmap, string pszFilename, uint nLineNum, string pszTaskItemText)
        {
            throw new NotImplementedException();
        }

        public int OutputTaskItemStringEx(string pszOutputString, VSTASKPRIORITY nPriority, VSTASKCATEGORY nCategory, string pszSubcategory, int nBitmap, string pszFilename, uint nLineNum, string pszTaskItemText, string pszLookupKwd)
        {
            throw new NotImplementedException();
        }

        public int SetName(string pszPaneName)
        {
            _name = pszPaneName;
            return VSConstants.S_OK;
        }
    }
}

