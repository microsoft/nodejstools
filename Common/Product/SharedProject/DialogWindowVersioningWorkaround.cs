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

using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudioTools {
    /// <summary>
    /// Works around an issue w/ DialogWindow and targetting multiple versions of VS.
    /// 
    /// Because the Microsoft.VisualStudio.Shell.version.0 assembly changes names
    /// we cannot refer to both v10 and v11 versions from within the same XAML file.
    /// Instead we use this subclass defined in our assembly.
    /// </summary>
    class DialogWindowVersioningWorkaround : DialogWindow {
    }
}
