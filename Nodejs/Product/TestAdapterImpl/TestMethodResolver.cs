// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter
{
    [Export(typeof(ITestMethodResolver))]
    internal class TestMethodResolver : ITestMethodResolver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TestContainerDiscoverer discoverer;

        #region ITestMethodResolver Members

        [ImportingConstructor]
        public TestMethodResolver([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            [Import]TestContainerDiscoverer discoverer)
        {
            this.serviceProvider = serviceProvider;
            this.discoverer = discoverer;
        }

        public Uri ExecutorUri=> NodejsConstants.ExecutorUri;

        public string GetCurrentTest(string filePath, int line, int lineCharOffset)
        {
            var project = this.PathToProject(filePath);
            if (project != null && this.discoverer.IsProjectKnown(project))
            {
                var buildEngine = new MSBuild.ProjectCollection();
                if (project.TryGetProjectPath(out var projectPath))
                {
                    var proj = buildEngine.LoadProject(projectPath);

                    //TODO - Find the matching function
                    /*
                    Need identify which method is executing.
                     */
                }
            }
            return null;
        }

        private IVsProject PathToProject(string filePath)
        {
            var rdt = (IVsRunningDocumentTable)this.serviceProvider.GetService(typeof(SVsRunningDocumentTable));
            IVsHierarchy hierarchy;

            var docData = IntPtr.Zero;
            try
            {
                var hr = rdt.FindAndLockDocument(
                    (uint)_VSRDTFLAGS.RDT_NoLock,
                    filePath,
                    out hierarchy,
                    out _,
                    out docData,
                    out _);
                ErrorHandler.ThrowOnFailure(hr);
            }
            finally
            {
                if (docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                    docData = IntPtr.Zero;
                }
            }

            return hierarchy as IVsProject;
        }

        #endregion
    }
}
