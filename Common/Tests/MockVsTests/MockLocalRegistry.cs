// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockLocalRegistry : ILocalRegistry, ILocalRegistryCorrected
    {
        private static Guid AggregatorGuid = new Guid("{C402364C-5474-47e7-AE72-BF5418780221}");

        public int CreateInstance(Guid clsid, object punkOuter, ref Guid riid, uint dwFlags, out IntPtr ppvObj)
        {
            throw new NotImplementedException();
        }

        public int GetClassObjectOfClsid(ref Guid clsid, uint dwFlags, IntPtr lpReserved, ref Guid riid, out IntPtr ppvClassObject)
        {
            throw new NotImplementedException();
        }

        public int GetTypeLibOfClsid(Guid clsid, out VisualStudio.OLE.Interop.ITypeLib pptLib)
        {
            throw new NotImplementedException();
        }

        public int CreateInstance(Guid clsid, IntPtr punkOuterIUnknown, ref Guid riid, uint dwFlags, out IntPtr ppvObj)
        {
            if (clsid == typeof(Microsoft.VisualStudio.ProjectAggregator.CProjectAggregatorClass).GUID)
            {
                var res = new ProjectAggregator();
                ppvObj = Marshal.GetIUnknownForObject(res);
                return VSConstants.S_OK;
            }
            throw new NotImplementedException();
        }
    }
}

