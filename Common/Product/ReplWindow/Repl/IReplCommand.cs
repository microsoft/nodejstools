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

using System.Threading.Tasks;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
#if INTERACTIVE_WINDOW
    using IReplWindow = IInteractiveWindow;
#endif

    /// <summary>
    /// Represents a command which can be run from a REPL window.
    /// 
    /// This interface is a MEF contract and can be implemented and exported to add commands to the REPL window.
    /// </summary>
#if INTERACTIVE_WINDOW
    public interface IInteractiveWindowCommand {
#else
    public interface IReplCommand {
#endif
        /// <summary>
        /// Asynchronously executes the command with specified arguments and calls back the given completion when finished.
        /// </summary>
        /// <param name="window">The interactive window.</param>
        /// <returns>The task that completes the execution.</returns>
        Task<ExecutionResult> Execute(IReplWindow window, string arguments);

        /// <summary>
        /// Gets a description of the REPL command which is displayed when the user asks for help.
        /// </summary>
        string Description {
            get;
        }

        /// <summary>
        /// Gets the text for the actual command.
        /// </summary>
        string Command {
            get;
        }

        /// <summary>
        /// Content to be placed in a toolbar button or null if should not be placed on a toolbar.
        /// </summary>
        object ButtonContent {
            get;
        }
    }
}
