// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnStyle(int blockIndent)
        {
            if (this._cssClassifier != null)
            {
                var start = this._cs.Position;

                SkipToEndOfBlock(blockIndent, text: false);

                var end = this._cs.Position;
                var length = end - start;
                if (length > 0)
                {
                    this._cssBuffer.Replace(
                        new Span(0, this._cssBuffer.CurrentSnapshot.Length),
                        this._cs.Text.GetText(new TextRange(start, length))
                    );

                    var tokens = this._cssClassifier.GetClassificationSpans(new SnapshotSpan(this._cssBuffer.CurrentSnapshot, 0, this._cssBuffer.CurrentSnapshot.Length));
                    foreach (var t in tokens)
                    {
                        AddToken(t.ClassificationType, t.Span.Start.Position + start, t.Span.Length);
                    }
                }
            }
        }
    }
}
