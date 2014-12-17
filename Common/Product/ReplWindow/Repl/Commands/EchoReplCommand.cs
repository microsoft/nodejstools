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

using System.ComponentModel.Composition;
using System.Threading.Tasks;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
#if INTERACTIVE_WINDOW
    using IReplCommand = IInteractiveWindowCommand;
    using IReplWindow = IInteractiveWindow;
#endif

    [Export(typeof(IReplCommand))]
    class EchoReplCommand : IReplCommand {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments) {
            arguments = arguments.ToLowerInvariant();
            if (arguments == "on") {
                window.SetOptionValue(ReplOptions.ShowOutput, true);
            } else {
                window.SetOptionValue(ReplOptions.ShowOutput, false);
            }
            return ExecutionResult.Succeeded;
        }

        public string Description {
            get { return "Suppress or unsuppress output to the buffer"; }
        }

        public string Command {
            get { return "echo"; }
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}
