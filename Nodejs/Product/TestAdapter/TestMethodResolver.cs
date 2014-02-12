/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.TestAdapter;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter {
    [Export(typeof(ITestMethodResolver))]
    class TestMethodResolver : ITestMethodResolver {
        private readonly IServiceProvider _serviceProvider;
        private readonly TestContainerDiscoverer _discoverer;
        
        #region ITestMethodResolver Members

        [ImportingConstructor]
        public TestMethodResolver([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            [Import]TestContainerDiscoverer discoverer) {
            _serviceProvider = serviceProvider;
            _discoverer = discoverer;
        }

        public Uri ExecutorUri {
            get { return TestExecutor.ExecutorUri; }
        }

        public string GetCurrentTest(string filePath, int line, int lineCharOffset) {
            var project = PathToProject(filePath);
            if (project != null && _discoverer.IsProjectKnown(project)) {
                var buildEngine = new MSBuild.ProjectCollection();
                string projectPath;
                if (project.TryGetProjectPath(out projectPath)) {
                    var proj = buildEngine.LoadProject(projectPath);

                    //TODO - Find the matching function
                    /*
                    Need identify which method is executing.
                     */
                }
            }
            return null;
        }

        private IVsProject PathToProject(string filePath) {
            var rdt = (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));
            IVsHierarchy hierarchy;
            uint itemId;
            IntPtr docData = IntPtr.Zero;
            uint cookie;
            try {
                var hr = rdt.FindAndLockDocument(
                    (uint)_VSRDTFLAGS.RDT_NoLock,
                    filePath,
                    out hierarchy,
                    out itemId,
                    out docData,
                    out cookie);
                ErrorHandler.ThrowOnFailure(hr);
            } finally {
                if (docData != IntPtr.Zero) {
                    Marshal.Release(docData);
                    docData = IntPtr.Zero;
                }
            }

            return hierarchy as IVsProject;
        }

        #endregion
    }
}
