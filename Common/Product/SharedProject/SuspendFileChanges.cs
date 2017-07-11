// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// helper to make the editor ignore external changes
    /// </summary>
    internal class SuspendFileChanges
    {
        private string documentFileName;

        private bool isSuspending;

        private IServiceProvider site;

        private IVsDocDataFileChangeControl fileChangeControl;

        public SuspendFileChanges(IServiceProvider site, string document)
        {
            this.site = site;
            this.documentFileName = document;
        }

        public void Suspend()
        {
            if (this.isSuspending)
                return;

            var docData = IntPtr.Zero;
            try
            {
                var rdt = this.site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

                IVsHierarchy hierarchy;
                uint itemId;
                uint docCookie;
                IVsFileChangeEx fileChange;

                if (rdt == null)
                    return;

                ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, this.documentFileName, out hierarchy, out itemId, out docData, out docCookie));

                if ((docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL) || docData == IntPtr.Zero)
                    return;

                fileChange = this.site.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;

                if (fileChange != null)
                {
                    this.isSuspending = true;
                    ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, this.documentFileName, 1));
                    if (docData != IntPtr.Zero)
                    {
                        IVsPersistDocData persistDocData = null;

                        // if interface is not supported, return null
                        var unknown = Marshal.GetObjectForIUnknown(docData);
                        if (unknown is IVsPersistDocData)
                        {
                            persistDocData = (IVsPersistDocData)unknown;
                            if (persistDocData is IVsDocDataFileChangeControl)
                            {
                                this.fileChangeControl = (IVsDocDataFileChangeControl)persistDocData;
                                if (this.fileChangeControl != null)
                                {
                                    ErrorHandler.ThrowOnFailure(this.fileChangeControl.IgnoreFileChanges(1));
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidCastException e)
            {
                Trace.WriteLine("Exception" + e.Message);
            }
            finally
            {
                if (docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                }
            }
            return;
        }

        public void Resume()
        {
            if (!this.isSuspending)
                return;
            IVsFileChangeEx fileChange;
            fileChange = this.site.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
            if (fileChange != null)
            {
                this.isSuspending = false;
                ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, this.documentFileName, 0));
                if (this.fileChangeControl != null)
                {
                    ErrorHandler.ThrowOnFailure(this.fileChangeControl.IgnoreFileChanges(0));
                }
            }
        }
    }
}
