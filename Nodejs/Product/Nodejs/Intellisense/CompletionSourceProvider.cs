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
using System.ComponentModel.Composition;
using Microsoft.NodejsTools.Classifier;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Intellisense {
    [Export(typeof(ICompletionSourceProvider)), ContentType(NodejsConstants.Nodejs), Order, Name("Node.js Completion Source")]
    sealed class CompletionSourceProvider : ICompletionSourceProvider {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGlyphService _glyphService;

        [ImportingConstructor]
        public CompletionSourceProvider([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            IGlyphService glyphService) {
            _serviceProvider = serviceProvider;
            _glyphService = glyphService;
        }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) {
            NodejsClassifier classifier;
            if (textBuffer.Properties.TryGetProperty<NodejsClassifier>(typeof(NodejsClassifier), out classifier)) {
                return new CompletionSource(
                    textBuffer,
                    classifier,
                    _serviceProvider,
                    _glyphService
                );
            }
            return null;
        }

        #endregion
    }
}
