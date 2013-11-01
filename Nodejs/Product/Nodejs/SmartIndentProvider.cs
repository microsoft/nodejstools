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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
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
            [ImportMany(typeof(ITaggerProvider))]Lazy<ITaggerProvider, ClassifierMetadata>[] classifierProviders) {
            _editorOptionsFactory = editorOptionsFactory;

            // we use a tagger provider here instead of an IClassifierProvider because the 
            // JS language service doesn't actually implement IClassifierProvider and instead implemnets
            // ITaggerProvider<ClassificationTag> instead.  We can get those tags via IClassifierAggregatorService
            // but that merges together adjacent tokens of the same type, so we go straight to the
            // source here.
            _taggerProvider = classifierProviders.Where(
                provider =>
                    provider.Metadata.ContentTypes.Contains(NodejsConstants.JavaScript) &&
                    provider.Metadata.TagTypes.Any(tagType => tagType.IsSubclassOf(typeof(ClassificationTag)))
            ).First().Value;
        }

        #region ISmartIndentProvider Members

        public ISmartIndent CreateSmartIndent(ITextView textView) {
            return new SmartIndent(
                textView,
                _editorOptionsFactory.GetOptions(textView),
                _taggerProvider.CreateTagger<ClassificationTag>(textView.TextBuffer)
            );
        }

        #endregion
    }

    [Export(typeof(ISmartIndentProvider))]
    [ContentType(NodejsConstants.NodejsRepl)]
    [Name("NodejsReplSmartIndent")]
    class ReplSmartIndentProvider : ISmartIndentProvider {
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;
        private readonly ITaggerProvider _taggerProvider;
        [ImportingConstructor]
        public ReplSmartIndentProvider(
            IEditorOptionsFactoryService editorOptionsFactory,
            [ImportMany(typeof(ITaggerProvider))]Lazy<ITaggerProvider, ClassifierMetadata>[] classifierProviders
            ) {
            _editorOptionsFactory = editorOptionsFactory;

            // we use a tagger provider here instead of an IClassifierProvider because the 
            // JS language service doesn't actually implement IClassifierProvider and instead implemnets
            // ITaggerProvider<ClassificationTag> instead.  We can get those tags via IClassifierAggregatorService
            // but that merges together adjacent tokens of the same type, so we go straight to the
            // source here.
            _taggerProvider = classifierProviders.Where(
                provider =>
                    provider.Metadata.ContentTypes.Contains(NodejsConstants.JavaScript) &&
                    provider.Metadata.TagTypes.Any(tagType => tagType.IsSubclassOf(typeof(ClassificationTag)))
            ).First().Value;
        }

        #region ISmartIndentProvider Members

        public ISmartIndent CreateSmartIndent(ITextView textView) {
            return new SmartIndent(
                textView,
                _editorOptionsFactory.GetOptions(textView),
                _taggerProvider.CreateTagger<ClassificationTag>(textView.TextBuffer)
            );
        }

        #endregion
    }
}
