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
