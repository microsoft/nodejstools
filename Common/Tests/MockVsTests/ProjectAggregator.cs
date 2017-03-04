// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class ProjectAggregator : IVsProjectAggregator2, ICustomQueryInterface
    {
        private IntPtr _inner;
        private IntPtr _project;

        public int SetInner(IntPtr innerIUnknown)
        {
            _inner = innerIUnknown;
            return VSConstants.S_OK;
        }

        public int SetMyProject(IntPtr projectIUnknown)
        {
            _project = projectIUnknown;
            return VSConstants.S_OK;
        }

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            if (_project != IntPtr.Zero)
            {
                if (ErrorHandler.Succeeded(Marshal.QueryInterface(_project, ref iid, out ppv)))
                {
                    return CustomQueryInterfaceResult.Handled;
                }
            }
            if (_inner != IntPtr.Zero)
            {
                if (ErrorHandler.Succeeded(Marshal.QueryInterface(_inner, ref iid, out ppv)))
                {
                    return CustomQueryInterfaceResult.Handled;
                }
            }
            ppv = IntPtr.Zero;
            return CustomQueryInterfaceResult.Failed;
        }
    }
}

