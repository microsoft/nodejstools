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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
#if INTERACTIVE_WINDOW
    using IReplWindow = IInteractiveWindow;
#endif

    /// <summary>
    /// Provides access to creating or finding existing REPL windows.   
    /// </summary>
#if INTERACTIVE_WINDOW
    public interface IInteractiveWindowProvider {
#else
    public interface IReplWindowProvider {
#endif
        /// <summary>
        /// Creates a REPL window and returns a ToolWindowPane which implements IReplWindow.  An IReplEvaluatorProvider must exist
        /// to respond and create the specified REPL ID.
        /// 
        /// The returned object is also a ToolWindowPane and can be cast for access to control the docking with VS.
        /// </summary>
        IReplWindow CreateReplWindow(IContentType/*!*/ contentType, string/*!*/ title, Guid languageServiceGuid, string replId);

        /// <summary>
        /// Finds the REPL w/ the specified ID or returns null if the window hasn't been created.  An IReplEvaluatorProvider must exist
        /// to respond and create the specified REPL ID.
        /// 
        /// The returned object is also a ToolWindowPane and can be cast for access to control the docking with VS.
        /// </summary>
        IReplWindow FindReplWindow(string replId);

        /// <summary>
        /// Returns this list of repl windows currently loaded.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IReplWindow> GetReplWindows();
    }
}
