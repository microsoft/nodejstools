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
using System.Linq;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools {
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(NodejsConstants.Nodejs)]
    [Name("NodejsSmartIndent")]
    class SmartIndentProvider : ISmartIndentProvider {
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;
        private readonly ITaggerProvider _taggerProvider;
        [ImportingConstructor]
        public SmartIndentProvider(IEditorOptionsFactoryService editorOptionsFactory,
            [ImportMany(typeof(ITaggerProvider))]Lazy<ITaggerProvider, TaggerProviderMetadata>[] classifierProviders) {
            _editorOptionsFactory = editorOptionsFactory;

            // we use a tagger provider here instead of an IClassifierProvider because the 
            // JS language service doesn't actually implement IClassifierProvider and instead implemnets
            // ITaggerProvider<ClassificationTag> instead.  We can get those tags via IClassifierAggregatorService
            // but that merges together adjacent tokens of the same type, so we go straight to the
            // source here.
            var foundProvider = classifierProviders.Where(
                provider =>
                    provider.Metadata.ContentTypes.Contains(NodejsConstants.JavaScript) &&
                    provider.Metadata.TagTypes.Any(tagType => tagType.IsSubclassOf(typeof(ClassificationTag)))
                ).FirstOrDefault();

            _taggerProvider = foundProvider == null ? null : foundProvider.Value;
        }

        #region ISmartIndentProvider Members

        public ISmartIndent CreateSmartIndent(ITextView textView) {
            if (_taggerProvider == null)
                return null;

            return new SmartIndent(
                textView,
                _editorOptionsFactory.GetOptions(textView),
                _taggerProvider.CreateTagger<ClassificationTag>(textView.TextBuffer));
        }

        #endregion
    }
}
