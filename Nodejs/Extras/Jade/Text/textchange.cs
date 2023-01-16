// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Jade
{
    internal class TextChange : ICloneable
    {
        /// <summary>
        /// Text snapshot version
        /// </summary>
        public int Version;

        /// <summary>
        /// Changed range in the old snapshot.
        /// </summary>
        public TextRange OldRange;

        /// <summary>
        /// Changed range in the current snapshot.
        /// </summary>
        public TextRange NewRange;

        /// <summary>
        /// Previuos text snapshot
        /// </summary>
        public ITextProvider OldText;

        /// <summary>
        /// Current text snapshot
        /// </summary>
        public ITextProvider NewText;

        public TextChange()
        {
            Clear();
        }

        public TextChange(int start, int oldLength, int newLength) :
            this()
        {
            this.OldRange = new TextRange(start, start + oldLength);
            this.NewRange = new TextRange(start, start + newLength);
        }

        public TextChange(int start, int oldLength, int newLength, ITextProvider oldText, ITextProvider newText)
            : this()
        {
            this.Combine(new TextChange(start, oldLength, newLength));

            this.OldText = oldText;
            this.NewText = newText;
        }

        public TextChange(TextChange change, ITextProvider oldText, ITextProvider newText)
            : this()
        {
            this.Combine(change);

            this.OldText = oldText;
            this.NewText = newText;
        }

        public virtual void Clear()
        {
            this.OldRange = TextRange.EmptyRange;
            this.NewRange = TextRange.EmptyRange;

            this.OldText = null;
            this.NewText = null;
        }

        /// <summary>
        /// True if no changes are pending.
        /// </summary>
        public virtual bool IsEmpty()
        {
            return (this.OldRange.Length == 0 && this.NewRange.Length == 0);
        }

        public void Combine(TextChange other)
        {
            if (other.IsEmpty())
            {
                return;
            }

            if (this.OldRange == TextRange.EmptyRange || this.NewRange == TextRange.EmptyRange)
            {
                this.OldRange = other.OldRange;
                this.NewRange = other.NewRange;
            }
            else
            {
                var oldStart = Math.Min(other.OldRange.Start, this.OldRange.Start);
                var oldEnd = Math.Max(other.OldRange.End, this.OldRange.End);

                var newStart = Math.Min(other.NewRange.Start, this.NewRange.Start);
                var newEnd = Math.Max(other.NewRange.End, this.NewRange.End);

                this.OldRange = new TextRange(oldStart, oldEnd);
                this.NewRange = new TextRange(newStart, newEnd);
            }

            this.Version = Math.Max(this.Version, other.Version);
        }

        #region ICloneable Members
        public object Clone()
        {
            var clone = this.MemberwiseClone() as TextChange;

            clone.OldRange = this.OldRange.Clone() as TextRange;
            clone.NewRange = this.NewRange.Clone() as TextRange;

            return clone;
        }
        #endregion
    }
}
