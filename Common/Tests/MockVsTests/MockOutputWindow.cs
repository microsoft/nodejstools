// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockOutputWindow : IVsOutputWindow
    {
        private static Dictionary<Guid, MockOutputWindowPane> _panes = new Dictionary<Guid, MockOutputWindowPane>() {
            {VSConstants.OutputWindowPaneGuid.GeneralPane_guid, new MockOutputWindowPane("General") }
        };

        public int CreatePane(ref Guid rguidPane, string pszPaneName, int fInitVisible, int fClearWithSolution)
        {
            MockOutputWindowPane pane;
            if (_panes.TryGetValue(rguidPane, out pane))
            {
                _panes[rguidPane] = new MockOutputWindowPane(pszPaneName);
            }
            return VSConstants.S_OK;
        }

        public int DeletePane(ref Guid rguidPane)
        {
            _panes.Remove(rguidPane);
            return VSConstants.S_OK;
        }

        public int GetPane(ref Guid rguidPane, out IVsOutputWindowPane ppPane)
        {
            MockOutputWindowPane pane;
            if (_panes.TryGetValue(rguidPane, out pane))
            {
                ppPane = pane;
                return VSConstants.S_OK;
            }
            ppPane = null;
            return VSConstants.E_FAIL;
        }
    }
}

