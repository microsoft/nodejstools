// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockCodeWindow : IVsCodeWindow, Microsoft.VisualStudio.OLE.Interop.IConnectionPointContainer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextView _view;

        public MockCodeWindow(IServiceProvider serviceProvider, ITextView view)
        {
            _serviceProvider = serviceProvider;
            _view = view;
        }

        public int Close()
        {
            throw new NotImplementedException();
        }

        public int GetBuffer(out IVsTextLines ppBuffer)
        {
            throw new NotImplementedException();
        }

        public int GetEditorCaption(READONLYSTATUS dwReadOnly, out string pbstrEditorCaption)
        {
            throw new NotImplementedException();
        }

        public int GetLastActiveView(out IVsTextView ppView)
        {
            throw new NotImplementedException();
        }

        public int GetPrimaryView(out IVsTextView ppView)
        {
            var compModel = (IComponentModel)_serviceProvider.GetService(typeof(SComponentModel));
            var editorAdapters = compModel.GetService<IVsEditorAdaptersFactoryService>();
            ppView = editorAdapters.GetViewAdapter(_view);
            return VSConstants.S_OK;
        }

        public int GetSecondaryView(out IVsTextView ppView)
        {
            ppView = null;
            return VSConstants.E_FAIL;
        }

        public int GetViewClassID(out Guid pclsidView)
        {
            throw new NotImplementedException();
        }

        public int SetBaseEditorCaption(string[] pszBaseEditorCaption)
        {
            throw new NotImplementedException();
        }

        public int SetBuffer(IVsTextLines pBuffer)
        {
            throw new NotImplementedException();
        }

        public int SetViewClassID(ref Guid clsidView)
        {
            throw new NotImplementedException();
        }

        public void EnumConnectionPoints(out VisualStudio.OLE.Interop.IEnumConnectionPoints ppEnum)
        {
            throw new NotImplementedException();
        }

        public void FindConnectionPoint(ref Guid riid, out VisualStudio.OLE.Interop.IConnectionPoint ppCP)
        {
            ppCP = null;
        }
    }
}

