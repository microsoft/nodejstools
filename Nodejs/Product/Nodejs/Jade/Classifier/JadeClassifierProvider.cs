/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
            [ImportMany(typeof(IClassifierProvider))]Lazy<IClassifierProvider, ClassifierProviderMetadata>[] classifierProviders) {
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
