// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockVsShell : IVsShell
    {
        public int AdviseBroadcastMessages(IVsBroadcastMessageEvents pSink, out uint pdwCookie)
        {
            throw new NotImplementedException();
        }

        public int AdviseShellPropertyChanges(IVsShellPropertyEvents pSink, out uint pdwCookie)
        {
            throw new NotImplementedException();
        }

        public int GetPackageEnum(out IEnumPackages ppenum)
        {
            throw new NotImplementedException();
        }

        public int GetProperty(int propid, out object pvar)
        {
            pvar = null;
            return VSConstants.E_FAIL;
        }

        public int IsPackageInstalled(ref Guid guidPackage, out int pfInstalled)
        {
            throw new NotImplementedException();
        }

        public int IsPackageLoaded(ref Guid guidPackage, out IVsPackage ppPackage)
        {
            throw new NotImplementedException();
        }

        public int LoadPackage(ref Guid guidPackage, out IVsPackage ppPackage)
        {
            throw new NotImplementedException();
        }

        public int LoadPackageString(ref Guid guidPackage, uint resid, out string pbstrOut)
        {
            throw new NotImplementedException();
        }

        public int LoadUILibrary(ref Guid guidPackage, uint dwExFlags, out uint phinstOut)
        {
            throw new NotImplementedException();
        }

        public int SetProperty(int propid, object var)
        {
            throw new NotImplementedException();
        }

        public int UnadviseBroadcastMessages(uint dwCookie)
        {
            throw new NotImplementedException();
        }

        public int UnadviseShellPropertyChanges(uint dwCookie)
        {
            throw new NotImplementedException();
        }
    }
}

