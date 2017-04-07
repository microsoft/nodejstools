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
using System.Resources;
using System.Threading;
using CommonSR = Microsoft.VisualStudioTools.Project.SR;

namespace Microsoft.NodejsTools.Project {
    internal class SR : CommonSR {
        internal const string NodejsToolsForVisualStudio = "NodejsToolsForVisualStudio";

        internal const string NodeExeDoesntExist = "NodeExeDoesntExist";
        internal const string NodejsNotInstalled = "NodejsNotInstalled";
#if !DEV15
        internal const string NodejsVersionNotSupported = "NodejsVersionNotSupported";
#endif
        internal const string TestFramework = "TestFramework";

        private static readonly Lazy<ResourceManager> _manager = new Lazy<ResourceManager>(
            () => new System.Resources.ResourceManager("Microsoft.NodejsTools.Resources", typeof(SR).Assembly),
            LazyThreadSafetyMode.ExecutionAndPublication
        );

        private static ResourceManager Manager {
            get {
                return _manager.Value;
            }
        }

        internal static new string GetString(string value, params object[] args) {
            return GetStringInternal(Manager, value, args) ?? CommonSR.GetString(value, args);
        }

        internal static string ProductName {
            get {
                return GetString(NodejsToolsForVisualStudio);
            }
        }
    }
}
