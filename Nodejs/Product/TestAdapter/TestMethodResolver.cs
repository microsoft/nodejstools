//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudioTools.TestAdapter;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.TestAdapter
{
    [Export(typeof(ITestMethodResolver))]
    internal class TestMethodResolver : ITestMethodResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TestContainerDiscoverer _discoverer;

        #region ITestMethodResolver Members

        [ImportingConstructor]
        public TestMethodResolver([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            [Import]TestContainerDiscoverer discoverer)
        {
            _serviceProvider = serviceProvider;
            _discoverer = discoverer;
        }

        public Uri ExecutorUri
        {
            get { return TestExecutor.ExecutorUri; }
        }

        public string GetCurrentTest(string filePath, int line, int lineCharOffset)
        {
            var project = PathToProject(filePath);
            if (project != null && _discoverer.IsProjectKnown(project))
            {
                var buildEngine = new MSBuild.ProjectCollection();
                string projectPath;
                if (project.TryGetProjectPath(out projectPath))
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
            var rdt = (IVsRunningDocumentTable)_serviceProvider.GetService(typeof(SVsRunningDocumentTable));
            IVsHierarchy hierarchy;
            uint itemId;
            IntPtr docData = IntPtr.Zero;
            uint cookie;
            try
            {
                var hr = rdt.FindAndLockDocument(
                    (uint)_VSRDTFLAGS.RDT_NoLock,
                    filePath,
                    out hierarchy,
                    out itemId,
                    out docData,
                    out cookie);
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
