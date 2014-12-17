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
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Navigation {
    /// <summary>
    /// This interface defines the service that finds current language files inside a hierarchy
    /// and builds information to expose to the class view or object browser.
    /// </summary>    
    internal interface ILibraryManager {
        void RegisterHierarchy(IVsHierarchy hierarchy);
        void UnregisterHierarchy(IVsHierarchy hierarchy);
        void RegisterLineChangeHandler(uint document, TextLineChangeEvent lineChanged, Action<IVsTextLines> onIdle);
    }
    internal delegate void TextLineChangeEvent(object sender, TextLineChange[] changes, int last);
}
