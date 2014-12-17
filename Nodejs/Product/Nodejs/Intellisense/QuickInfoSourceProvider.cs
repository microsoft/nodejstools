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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Intellisense {
    [Export(typeof(IQuickInfoSourceProvider)), ContentType(NodejsConstants.Nodejs), Order, Name("Node.js Quick Info Source")]
    class QuickInfoSourceProvider : IQuickInfoSourceProvider {
        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) {
            return new QuickInfoSource(this, textBuffer);
        }
    }
}
