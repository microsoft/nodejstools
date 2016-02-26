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
using System.IO;

namespace Microsoft.NodejsTools {
    static class NodejsFileExtensions {
        public static bool IsJavaScriptFile(string file) {
            return IsFileType(file, NodejsConstants.JavaScriptExtension);
        }

        private static bool IsFileType(string file, string extension) {
            var ext = Path.GetExtension(file);
            return String.Equals(ext, extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
