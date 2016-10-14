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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Project.NewFileMenuGroup {
    internal static class NewFileUtilities {
        private static string GetInitialName(string fileType) {
            switch (fileType) {
                case NodejsConstants.JavaScript:
                    return "JavaScript.js";
                case NodejsConstants.TypeScript:
                    return "TypeScript.ts";
                case NodejsConstants.HTML:
                    return "HTML.html";
                case NodejsConstants.CSS:
                    return "CSS.css";
                default:
                    Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Invalid file type: {0}", fileType));
                    return null;
            }
        }

        private static void CreateNewFile(NodejsProjectNode projectNode, uint containerId, string fileType) {
            using (var dialog = new NewFileNameForm(GetInitialName(fileType))) {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    string itemName = dialog.TextBox.Text;

                    VSADDRESULT[] pResult = new VSADDRESULT[1];
                    projectNode.AddItem(
                        containerId,                                 // Identifier of the container folder. 
                        VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE,    // Indicate that we want to create this new file by cloning a template file.
                        itemName,
                        1,                                           // Number of templates in the next parameter. Must be 1 if using VSADDITEMOP_CLONEFILE.
                        new string[] { Path.GetTempFileName() },     // Array contains the template file path.
                        IntPtr.Zero,                                 // Handle to the Add Item dialog box. Must be Zero if using VSADDITEMOP_CLONEFILE.
                        pResult);
                }
            }
        }

        internal static void CreateNewJavaScriptFile(NodejsProjectNode projectNode, uint containerId) {
            CreateNewFile(projectNode, containerId, NodejsConstants.JavaScript);
        }

        internal static void CreateNewTypeScriptFile(NodejsProjectNode projectNode, uint containerId) {
            CreateNewFile(projectNode, containerId, NodejsConstants.TypeScript);
        }

        internal static void CreateNewHTMLFile(NodejsProjectNode projectNode, uint containerId) {
            CreateNewFile(projectNode, containerId, NodejsConstants.HTML);
        }

        internal static void CreateNewCSSFile(NodejsProjectNode projectNode, uint containerId) {
            CreateNewFile(projectNode, containerId, NodejsConstants.CSS);
        }
    }
}
