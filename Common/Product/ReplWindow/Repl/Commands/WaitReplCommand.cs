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
using System.Threading.Tasks;
using System.Windows.Threading;

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
    class WaitReplCommand : IReplCommand {
        #region IReplCommand Members

        public Task<ExecutionResult> Execute(IReplWindow window, string arguments) {
            var delay = new TimeSpan(0, 0, 0, 0, int.Parse(arguments));
            var start = DateTime.UtcNow;
            while ((start + delay) > DateTime.UtcNow) {
                var frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action<DispatcherFrame>(f => f.Continue = false),
                    frame
                    );
                Dispatcher.PushFrame(frame);
            }
            return ExecutionResult.Succeeded;
        }

        public string Description {
            get { return "Wait for at least the specified number of milliseconds"; }
        }

        public string Command {
            get { return "wait"; }
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}
