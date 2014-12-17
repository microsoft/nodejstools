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
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools {
    /// <summary>
    /// Base class for available commands.  To add a new command you must first update the .vsct file so that
    /// our commands are registered and available.  Then you need to subclass Command and at a new instance of
    /// the command in CommandTable.  PythonToolsPackage will then register the command on startup.
    /// </summary>
    internal abstract class Command {
        /// <summary>
        /// Provides the implementation of what should happen when the command is executed.
        /// 
        /// sender is the MenuCommand or OleMenuCommand object which is causing the event to be fired.
        /// </summary>
        public abstract void DoCommand(object sender, EventArgs args);

        /// <summary>
        /// Enables a command to hook into our edit filter for Python text buffers.
        /// 
        /// Called with the OLECMD object for the command being processed.  Returns null
        /// if the command does not want to handle this message or the HRESULT that
        /// should be returned from the QueryStatus call.
        /// </summary>
        public virtual int? EditFilterQueryStatus(ref OLECMD cmd, IntPtr pCmdText) {
            return null;
        }

        /// <summary>
        /// Provides the CommandId for this command which corresponds to the CommandId in the vsct file
        /// and PkgCmdId.cs.
        /// </summary>
        public abstract int CommandId {
            get;
        }

        /// <summary>
        /// Provides an event handler that will be invoked before the menu containing the command
        /// is displayed.  This can enable, disable, or hide the menu command.  By default returns
        /// null.
        /// </summary>
        public virtual EventHandler BeforeQueryStatus {
            get {
                return null;
            }
        }
    }
}
