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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade {
    [Export(typeof(IClassifierProvider))]
    [ContentType(JadeContentTypeDefinition.JadeContentType)]
    internal sealed class JadeClassifierProvider : IClassifierProvider {
        public readonly IClassificationTypeRegistryService ClassificationRegistryService;
        public readonly ITaggerProvider JsTaggerProvider;
        public readonly IClassifierProvider CssClassifierProvider;
        public readonly ITextBufferFactoryService BufferFactoryService;
        public readonly IContentType JsContentType, CssContentType;

        private const string JavaScriptContentType = "JavaScript";
        [ImportingConstructor]
        public JadeClassifierProvider(IClassificationTypeRegistryService registryService,   
            ITextBufferFactoryService bufferFact,
            IContentTypeRegistryService contentTypeService,
            [ImportMany(typeof(ITaggerProvider))]Lazy<ITaggerProvider, TaggerProviderMetadata>[] taggerProviders,
            [ImportMany(typeof(IClassifierProvider))]Lazy<IClassifierProvider, IClassifierProviderMetadata>[] classifierProviders) {
            ClassificationRegistryService = registryService;
            BufferFactoryService = bufferFact;
            JsContentType = contentTypeService.GetContentType(NodejsConstants.JavaScript);
            CssContentType = contentTypeService.GetContentType(NodejsConstants.CSS);

            var jsTagger = taggerProviders.Where(
                provider =>
                    provider.Metadata.ContentTypes.Contains(NodejsConstants.JavaScript) &&
                    provider.Metadata.TagTypes.Any(tagType => tagType.IsSubclassOf(typeof(ClassificationTag)))
            ).FirstOrDefault();
            if (JsTaggerProvider != null) {
                JsTaggerProvider = jsTagger.Value;
            }

            var cssTagger = classifierProviders.Where(
                provider => provider.Metadata.ContentTypes.Any(x => x.Equals("css", StringComparison.OrdinalIgnoreCase))
            ).FirstOrDefault();
            if (cssTagger != null) {
                CssClassifierProvider = cssTagger.Value;
            }
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            var classifier = ServiceManager.GetService<JadeClassifier>(textBuffer);

            if (classifier == null)
                classifier = new JadeClassifier(textBuffer, this);

            return classifier;
        }
    }
}
