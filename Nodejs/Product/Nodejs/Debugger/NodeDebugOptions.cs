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

namespace Microsoft.NodejsTools.Debugger
{
    [Flags]
    internal enum NodeDebugOptions
    {
        None,
        /// <summary>
        /// Passing this flag to the debugger will cause it to wait for input on an abnormal (non-zero)
        /// exit code.
        /// </summary>
        WaitOnAbnormalExit = 0x01,
        /// <summary>
        /// Passing this flag to the debugger will cause it to wait for input on a normal (zero) exit code.
        /// </summary>
        WaitOnNormalExit = 0x02,
        /// <summary>
        /// Passing this flag will cause output to standard out to be redirected via the debugger
        /// so it can be outputted in the Visual Studio debug output window.
        /// </summary>
        RedirectOutput = 0x04,

        /// <summary>
        /// Set if you do not want to create a window
        /// </summary>
        CreateNoWindow = 0x40
    }
}
