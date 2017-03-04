// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using EnvDTE;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockVsProfferCommands : IVsProfferCommands
    {
        public void AddCommandBar(string pszCmdBarName, vsCommandBarType dwType, object pCmdBarParent, uint dwIndex, out object ppCmdBar)
        {
            ppCmdBar = null;
        }

        public void AddCommandBarControl(string pszCmdNameCanonical, object pCmdBarParent, uint dwIndex, uint dwCmdType, out object ppCmdBarCtrl)
        {
            ppCmdBarCtrl = null;
        }

        public void AddNamedCommand(ref Guid pguidPackage, ref Guid pguidCmdGroup, string pszCmdNameCanonical, out uint pdwCmdId, string pszCmdNameLocalized, string pszBtnText, string pszCmdTooltip, string pszSatelliteDLL, uint dwBitmapResourceId, uint dwBitmapImageIndex, uint dwCmdFlagsDefault, uint cUIContexts, ref Guid rgguidUIContexts)
        {
            pdwCmdId = 0;
        }

        public object FindCommandBar(IntPtr pToolbarSet, ref Guid pguidCmdGroup, uint dwMenuId)
        {
            throw new NotImplementedException();
        }

        public void RemoveCommandBar(object pCmdBar)
        {
            throw new NotImplementedException();
        }

        public void RemoveCommandBarControl(object pCmdBarCtrl)
        {
            throw new NotImplementedException();
        }

        public void RemoveNamedCommand(string pszCmdNameCanonical)
        {
            throw new NotImplementedException();
        }

        public void RenameNamedCommand(string pszCmdNameCanonical, string pszCmdNameCanonicalNew, string pszCmdNameLocalizedNew)
        {
            throw new NotImplementedException();
        }
    }
}

