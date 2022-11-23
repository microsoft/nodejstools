// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    internal partial class JadeTokenizer : Tokenizer<JadeToken>
    {
        private void OnScript(int blockIndent)
        {
            if (this._jsTagger != null)
            {
                var start = this._cs.Position;

                SkipToEndOfBlock(blockIndent, text: false);

                var end = this._cs.Position;
                var length = end - start;

                if (length > 0)
                {
                    this._jsBuffer.Replace(
                        new Span(0, this._jsBuffer.CurrentSnapshot.Length),
                        this._cs.Text.GetText(new TextRange(start, length))
                    );

                    var tokens = this._jsTagger.GetTags(new NormalizedSnapshotSpanCollection(new SnapshotSpan(this._jsBuffer.CurrentSnapshot, 0, this._jsBuffer.CurrentSnapshot.Length)));

                    foreach (var t in tokens)
                    {
                        AddToken(t.Tag.ClassificationType, t.Span.Start.Position + start, t.Span.Length);
                    }
                }
            }
        }
    }
}
