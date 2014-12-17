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
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Stores cached information for a completion that we can easily transform
    /// into our completions.  Primarily this exists to account for whether or not
    /// the completion is happening with ', ", or no quotes while allowing us to
    /// cache the results we've previously calculated.
    /// </summary>
    class CompletionInfo {
        public readonly string DisplayText, Description;
        public readonly ImageSource Glyph;

        public CompletionInfo(string displayText, string description, ImageSource glyph) {
            DisplayText = displayText;
            Description = description;
            Glyph = glyph;
        }
    }
}
