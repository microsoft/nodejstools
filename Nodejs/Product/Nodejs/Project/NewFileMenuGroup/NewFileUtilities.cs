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
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Project.NewFileMenuGroup {
    internal static class NewFileUtilities {
        internal static void CreateNewFile(NodejsProjectNode projectNode, uint containerId) {
            using (var dialog = new NewFileNameForm("")) {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    string itemName = dialog.TextBox.Text;
                    if (string.IsNullOrWhiteSpace(itemName)) {
                        return;
                    }
                    itemName = itemName.Trim();

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
    }
}
