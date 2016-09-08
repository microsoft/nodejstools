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
using Microsoft.VisualStudioTools;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Commands {
    internal sealed class SendFeedbackCommand : Command {
        public override void DoCommand(object sender, EventArgs args) {
            var uri = new Uri(@"https://aka.ms/ntvs-feedback");
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidSendFeedback; }
        }
    }
}
