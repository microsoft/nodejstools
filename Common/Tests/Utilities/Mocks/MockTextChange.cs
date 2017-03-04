// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;

namespace TestUtilities.Mocks
{
    internal class MockTextChange : ITextChange
    {
        private readonly SnapshotSpan _removed;
        private readonly string _inserted;
        private readonly int _newStart;
        private static readonly string[] NewLines = new[] { "\r\n", "\r", "\n" };

        public MockTextChange(SnapshotSpan removedSpan, int newStart, string insertedText)
        {
            _removed = removedSpan;
            _inserted = insertedText;
            _newStart = newStart;
        }

        public int Delta
        {
            get { return _inserted.Length - _removed.Length; }
        }

        public int LineCountDelta
        {
            get
            {
                return _inserted.Split(NewLines, StringSplitOptions.None).Length -
                    _removed.GetText().Split(NewLines, StringSplitOptions.None).Length;
            }
        }

        public int NewEnd
        {
            get
            {
                return NewPosition + _inserted.Length;
            }
        }

        public int NewLength
        {
            get { return _inserted.Length; }
        }

        public int NewPosition
        {
            get { return _newStart; }
        }

        public Span NewSpan
        {
            get
            {
                return new Span(NewPosition, NewLength);
            }
        }

        public string NewText
        {
            get { return _inserted; }
        }

        public int OldEnd
        {
            get { return _removed.End; }
        }

        public int OldLength
        {
            get { return _removed.Length; }
        }

        public int OldPosition
        {
            get { return _removed.Start; }
        }

        public Span OldSpan
        {
            get { return _removed.Span; }
        }

        public string OldText
        {
            get { return _removed.GetText(); }
        }
    }
}

