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

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
#if INTERACTIVE_WINDOW
    using IReplEvaluator = IInteractiveEngine;
#endif

    /// <summary>
    /// Supports a REPL evaluator which enables the user to switch between
    /// multiple scopes of execution.
    /// </summary>
    public interface IMultipleScopeEvaluator : IReplEvaluator {
        /// <summary>
        /// Sets the current scope to the given name.
        /// </summary>
        void SetScope(string scopeName);

        /// <summary>
        /// Gets the list of scopes which can be changed to.
        /// </summary>
        IEnumerable<string> GetAvailableScopes();

        /// <summary>
        /// Gets the current scope name.
        /// </summary>
        string CurrentScopeName {
            get;
        }

        /// <summary>
        /// Event is fired when the list of available scopes changes.
        /// </summary>
        event EventHandler<EventArgs> AvailableScopesChanged;

        /// <summary>
        /// Event is fired when support of multiple scopes has changed.
        /// </summary>
        event EventHandler<EventArgs> MultipleScopeSupportChanged;

        /// <summary>
        /// Returns true if multiple scope support is currently enabled, false if not.
        /// </summary>
        bool EnableMultipleScopes {
            get;
        }
    }
}
