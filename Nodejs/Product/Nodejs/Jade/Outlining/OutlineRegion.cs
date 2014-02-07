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
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Generic outlining region
    /// </summary>
    class OutlineRegion : TextRange {
        private ITextBuffer _textBuffer;
        private const string _outlineDisplayText = "...";

        public OutlineRegion(ITextBuffer textBuffer, ITextRange range)
            : this(textBuffer, range.Start, range.Length) {
        }

        public OutlineRegion(ITextBuffer textBuffer, int start, int length)
            : base(start, length) {
            _textBuffer = textBuffer;
        }

        public static OutlineRegion FromBounds(ITextBuffer textBuffer, int start, int end) {
            return new OutlineRegion(textBuffer, start, end - start);
        }

        /// <summary>
        /// Text to display in a tooltip when region is collapsed
        /// </summary>
        public virtual string HoverText {
            get {
                if (_textBuffer != null) {
                    int hoverTextLength = Math.Min(this.Length, 512);
                    hoverTextLength = Math.Min(hoverTextLength, _textBuffer.CurrentSnapshot.Length - this.Start);

                    var text = _textBuffer.CurrentSnapshot.GetText(this.Start, hoverTextLength);
                    if (hoverTextLength < this.Length)
                        text += "...";

                    return text;
                }

                return String.Empty;
            }
        }

        /// <summary>
        /// Text to display instead of a region when region is collapsed
        /// </summary>
        public virtual string DisplayText {
            get {
                return _outlineDisplayText;
            }
        }
    }
}
