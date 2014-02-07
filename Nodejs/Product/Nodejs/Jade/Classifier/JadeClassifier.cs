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
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) of VBScript items
    /// </summary>
    internal sealed class JadeClassifier : TokenBasedClassifier<JadeTokenType, JadeToken> {
        public JadeClassifier(ITextBuffer textBuffer, JadeClassifierProvider provider) :
            base(textBuffer, new JadeTokenizer(provider), new JadeClassificationNameProvider(provider.ClassificationRegistryService)) {
            LineBasedClassification = true;
            ServiceManager.AddService<JadeClassifier>(this, textBuffer);
        }

        protected override void RemoveSensitiveTokens(int position, TextRangeCollection<JadeToken> tokens) {
            if (tokens.Count > 0) {
                var line = TextBuffer.CurrentSnapshot.GetLineFromPosition(position);
                var index = tokens.GetFirstItemAfterPosition(line.Start);
                if (index >= 0) {
                    for (int i = index; i >= 0; i--) {
                        if (IsAnchorToken(tokens[i].TokenType)) {
                            line = TextBuffer.CurrentSnapshot.GetLineFromPosition(tokens[i].Start);
                            break;
                        }
                    }
                }

                int start = line.Start;
                int end = tokens[tokens.Count - 1].End;

                if (start < end)
                    tokens.RemoveInRange(TextRange.FromBounds(start, end), true);
            }

            base.RemoveSensitiveTokens(position, tokens);
        }

        protected override int GetAnchorPosition(int position) {
            var snapshot = TextBuffer.CurrentSnapshot;
            if (position < snapshot.Length) {
                var line = snapshot.GetLineFromPosition(position);
                position = line.Start;

                var index = Tokens.GetFirstItemAfterPosition(line.Start);
                if (index < 0)
                    index = Tokens.Count - 1;

                for (int i = index; i >= 0; i--) {
                    if (IsAnchorToken(Tokens[i].TokenType)) {
                        line = snapshot.GetLineFromPosition(Tokens[i].Start);
                        position = line.Start;
                        break;
                    }
                }

                return position;
            }

            return 0;
        }

        private static bool IsAnchorToken(JadeTokenType t) {
            switch (t) {
                case JadeTokenType.TagKeyword:
                case JadeTokenType.TagName:
                case JadeTokenType.Filter:
                case JadeTokenType.JadeSelector:
                case JadeTokenType.Comment:
                    return true;
            }

            return false;
        }
    }
}
