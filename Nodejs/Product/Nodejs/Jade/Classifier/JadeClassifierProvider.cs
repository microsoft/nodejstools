// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(JadeContentTypeDefinition.JadeContentType)]
    internal sealed class JadeClassifierProvider : IClassifierProvider
    {
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
            [ImportMany(typeof(IClassifierProvider))]Lazy<IClassifierProvider, IClassifierProviderMetadata>[] classifierProviders)
        {
            this.ClassificationRegistryService = registryService;
            this.BufferFactoryService = bufferFact;
            this.JsContentType = contentTypeService.GetContentType(NodejsConstants.JavaScript);
            this.CssContentType = contentTypeService.GetContentType(NodejsConstants.CSS);

            var jsTagger = taggerProviders.Where(
                provider =>
                    provider.Metadata.ContentTypes.Contains(NodejsConstants.JavaScript) &&
                    provider.Metadata.TagTypes.Any(tagType => tagType.IsSubclassOf(typeof(ClassificationTag)))
            ).FirstOrDefault();
            if (this.JsTaggerProvider != null)
            {
                this.JsTaggerProvider = jsTagger.Value;
            }

            var cssTagger = classifierProviders.Where(
                provider => provider.Metadata.ContentTypes.Any(x => x.Equals("css", StringComparison.OrdinalIgnoreCase))
            ).FirstOrDefault();
            if (cssTagger != null)
            {
                this.CssClassifierProvider = cssTagger.Value;
            }
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            var classifier = ServiceManager.GetService<JadeClassifier>(textBuffer);

            if (classifier == null)
            {
                classifier = new JadeClassifier(textBuffer, this);
            }

            return classifier;
        }
    }
}
