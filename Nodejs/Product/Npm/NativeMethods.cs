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

namespace Microsoft.NodejsTools.Npm {
    internal static class NativeMethods {
        public const int MAX_PATH = 260; // windef.h	
        public const int MAX_FOLDER_PATH = MAX_PATH - 12;   // folders need to allow 8.3 filenames, so MAX_PATH - 12
    }
}
