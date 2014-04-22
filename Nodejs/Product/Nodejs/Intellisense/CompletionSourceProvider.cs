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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Intellisense {
    [Export(typeof(ICompletionSourceProvider)), ContentType(NodejsConstants.Nodejs), Order, Name("Node.js Completion Source")]
    sealed class CompletionSourceProvider : ICompletionSourceProvider {
        private readonly IClassifierAggregatorService _classifierAggregator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGlyphService _glyphService;

        [ImportingConstructor]
        public CompletionSourceProvider(IClassifierAggregatorService classifierAggregator, 
            [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            IGlyphService glyphService) {
            _classifierAggregator = classifierAggregator;
            _serviceProvider = serviceProvider;
            _glyphService = glyphService;
        }

        #region ICompletionSourceProvider Members

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) {
            // only provide completions for our own buffers
            if (textBuffer.Properties.ContainsProperty(typeof(NodejsProjectionBuffer))) {
                return new CompletionSource(textBuffer, _classifierAggregator, _serviceProvider, _glyphService);
            }
            return null;
        }

        #endregion
    }
}
