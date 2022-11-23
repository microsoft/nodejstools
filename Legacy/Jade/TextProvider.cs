// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Text provider that implements ITextProvider over Visual Studio 
    /// core editor's ITextBuffer or ITextSnapshot 
    /// </summary>
    internal class TextProvider : ITextProvider, ITextSnapshotProvider
    {
        static public int BlockLength = 16384;

        private string _cachedBlock;
        private int _basePosition;
        private ITextSnapshot _snapshot;
        private bool _partial = false;

        public TextProvider(ITextSnapshot snapshot, bool partial = false)
        {
            this._snapshot = snapshot;
            this.Length = this._snapshot.Length;
            this._partial = partial;

            UpdateCachedBlock(0, partial ? BlockLength : snapshot.Length);
        }

        private void UpdateCachedBlock(int position, int length)
        {
            if (!this._partial && this._cachedBlock != null)
            {
                return;
            }

            if (this._cachedBlock == null || position < this._basePosition || (this._basePosition + this._cachedBlock.Length < position + length))
            {
                length = Math.Max(length, BlockLength);
                length = Math.Min(length, this._snapshot.Length - position);

                this._cachedBlock = this._snapshot.GetText(position, length);
                this._basePosition = position;
            }
        }

        public int Length { get; }

        public char this[int position]
        {
            get
            {
                if (position < 0 || position >= this.Length)
                {
                    return '\0';
                }

                UpdateCachedBlock(position, 1);
                return this._cachedBlock[position - this._basePosition];
            }
        }

        public string GetText(int position, int length)
        {
            UpdateCachedBlock(position, length);
            return this._cachedBlock.Substring(position - this._basePosition, length);
        }

        public string GetText(ITextRange range)
        {
            return GetText(range.Start, range.Length);
        }

        public int IndexOf(string text, int startPosition, bool ignoreCase)
        {
            return IndexOf(text, TextRange.FromBounds(startPosition, this.Length), ignoreCase);
        }

        public int IndexOf(string text, ITextRange range, bool ignoreCase)
        {
            for (var i = range.Start; i < range.End; i++)
            {
                var found = true;
                var k = i;
                int j;

                for (j = 0; j < text.Length && k < range.End; j++, k++)
                {
                    var ch1 = text[j];
                    var ch2 = this[k];

                    if (ignoreCase)
                    {
                        ch1 = char.ToLowerInvariant(ch1);
                        ch2 = char.ToLowerInvariant(ch2);
                    }

                    if (ch1 != ch2)
                    {
                        found = false;
                        break;
                    }
                }

                if (found && j == text.Length)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool CompareTo(int position, int length, string text, bool ignoreCase)
        {
            if (text.Length != length)
            {
                return false;
            }

            UpdateCachedBlock(position, Math.Max(length, text.Length));

            return string.Compare(this._cachedBlock, position - this._basePosition,
                                  text, 0, text.Length,
                                  ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;
        }

        public ITextProvider Clone()
        {
            return new TextProvider(this._snapshot, this._partial);
        }

        public int Version => this._snapshot.Version.VersionNumber;
#pragma warning disable 0067
        public event System.EventHandler<TextChangeEventArgs> OnTextChange;

        #region ITextSnapshotProvider

        public ITextSnapshot Snapshot => this._snapshot;
        #endregion
    }
}
