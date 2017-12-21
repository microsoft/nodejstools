// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Generic outlining region
    /// </summary>
    internal class OutlineRegion : TextRange
    {
        private ITextBuffer _textBuffer;
        private const string _outlineDisplayText = "...";

        public OutlineRegion(ITextBuffer textBuffer, ITextRange range)
            : this(textBuffer, range.Start, range.Length)
        {
        }

        public OutlineRegion(ITextBuffer textBuffer, int start, int length)
            : base(start, length)
        {
            this._textBuffer = textBuffer;
        }

        public static OutlineRegion FromBounds(ITextBuffer textBuffer, int start, int end)
        {
            return new OutlineRegion(textBuffer, start, end - start);
        }

        /// <summary>
        /// Text to display in a tooltip when region is collapsed
        /// </summary>
        public virtual string HoverText
        {
            get
            {
                if (this._textBuffer != null)
                {
                    var hoverTextLength = Math.Min(this.Length, 512);
                    hoverTextLength = Math.Min(hoverTextLength, this._textBuffer.CurrentSnapshot.Length - this.Start);

                    var text = this._textBuffer.CurrentSnapshot.GetText(this.Start, hoverTextLength);
                    if (hoverTextLength < this.Length)
                    {
                        text += "...";
                    }

                    return text;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Text to display instead of a region when region is collapsed
        /// </summary>
        public virtual string DisplayText => _outlineDisplayText;
    }
}
