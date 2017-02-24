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
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudioTools;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.Commands
{
    internal sealed class OpenRemoteDebugProxyFolderCommand : Command
    {
        private const string remoteDebugJsFileName = "RemoteDebug.js";

        private static string RemoteDebugProxyFolder
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "RemoteDebug");
            }
        }

        public override void DoCommand(object sender, EventArgs args)
        {
            // Open explorer to folder
            var remoteDebugProxyFolder = RemoteDebugProxyFolder;
            if (string.IsNullOrWhiteSpace(remoteDebugProxyFolder))
            {
                MessageBox.Show(Resources.RemoteDebugProxyFolderDoesNotExist, SR.ProductName);
                return;
            }

            var filePath = Path.Combine(remoteDebugProxyFolder, remoteDebugJsFileName);
            if (!File.Exists(filePath))
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.RemoteDebugProxyFileDoesNotExist, filePath), SR.ProductName);
            }
            else
            {
                Process.Start("explorer", string.Format(CultureInfo.InvariantCulture, "/e,/select,{0}", filePath));
            }
        }

        public override int CommandId
        {
            get { return (int)PkgCmdId.cmdidOpenRemoteDebugProxyFolder; }
        }
    }
}
