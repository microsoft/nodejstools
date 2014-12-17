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

namespace Microsoft.NodejsTools.Npm {
    [Flags]
    public enum PackageFlags {
        None = 0x0000,
        NotListedAsDependency = 0x0001,
        Missing = 0x0002,
        Dev = 0x0004,
        Optional = 0x0008,
        Bundled = 0x0010,
        VersionMismatch = 0x0100,
        Installed = 0x1000,
    }
}