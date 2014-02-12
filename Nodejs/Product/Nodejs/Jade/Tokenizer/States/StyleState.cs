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

using Microsoft.VisualStudio.Text;
namespace Microsoft.NodejsTools.Jade {
    internal partial class JadeTokenizer : Tokenizer<JadeToken> {
        private void OnStyle(int blockIndent) {
            if (_cssClassifier != null) {
                int start = _cs.Position;

                SkipToEndOfBlock(blockIndent, text: false);

                int end = _cs.Position;
                int length = end - start;
                if (length > 0) {
                    _cssBuffer.Replace(
                        new Span(0, _cssBuffer.CurrentSnapshot.Length),
                        _cs.Text.GetText(new TextRange(start, length))
                    );

                    var tokens = _cssClassifier.GetClassificationSpans(new SnapshotSpan(_cssBuffer.CurrentSnapshot, 0, _cssBuffer.CurrentSnapshot.Length));
                    foreach (var t in tokens) {
                        AddToken(t.ClassificationType, t.Span.Start.Position + start, t.Span.Length);
                    }
                }
            }
        }
    }
}