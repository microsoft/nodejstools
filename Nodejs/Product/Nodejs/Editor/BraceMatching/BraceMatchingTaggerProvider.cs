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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Editor.ShowBraces {
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(NodejsConstants.Nodejs)]
    [TagType(typeof(TextMarkerTag))]
    internal class BraceMatchingTaggerProvider : IViewTaggerProvider {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
            // Provide highlighting only on the top-level buffer.
            if (textView == null || textView.TextBuffer != buffer) {
                return null;
            }

            return new BraceMatchingTagger(textView, buffer) as ITagger<T>;
        }
    }
}
