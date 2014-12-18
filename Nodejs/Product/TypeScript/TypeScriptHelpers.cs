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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.NodejsTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TypeScript {
    internal static class TypeScriptHelpers {
        internal static bool IsTypeScriptFile(string filename) {
            return String.Equals(Path.GetExtension(filename), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase);
        }

        internal static string GetTypeScriptBackedJavaScriptFile(IVsProject project, string pathToFile) {

            string jsFilePath = Path.ChangeExtension(pathToFile, NodejsConstants.JavaScriptExtension);

            //Need to deal with the format being relative and explicit
            IVsBuildPropertyStorage props = (IVsBuildPropertyStorage)project;
            String outDir;
            ErrorHandler.ThrowOnFailure(props.GetPropertyValue("TypeScriptOutDir", null, 0, out outDir));

            if (String.IsNullOrEmpty(outDir)) {
                //No setting for OutDir
                //  .js file is created next to .ts file
                return jsFilePath;
            }

            string projHome = GetProjectHome(project);

            //Get the full path to outDir
            //  If outDir is rooted then outDirPath is going to be outDir ending with backslash
            string outDirPath = CommonUtils.GetAbsoluteDirectoryPath(projHome, outDir);

            //Find the relative path to the file from projectRoot
            //  This folder structure will be mirrored in the TypeScriptOutDir
            string relativeJSFilePath = CommonUtils.GetRelativeFilePath(projHome, jsFilePath);

            return Path.Combine(outDirPath, relativeJSFilePath);
        }

        private static string GetProjectHome(IVsProject project) {
            Debug.Assert(project != null);
            var hier = (IVsHierarchy)project;
            object extObject;
            ErrorHandler.ThrowOnFailure(hier.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out extObject
            ));
            var proj = extObject as EnvDTE.Project;
            if (proj == null) {
                return null;
            }
            var props = proj.Properties;
            if (props == null) {
                return null;
            }
            var projHome = props.Item("ProjectHome");
            if (projHome == null) {
                return null;
            }

            return projHome.Value as string;
        }
    }
}