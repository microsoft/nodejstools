// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace TestUtilities.Mocks
{
    public class MockTextCaret : ITextCaret
    {
        private MockTrackingPoint _position;
        private readonly MockTextView _view;

        public MockTextCaret(MockTextView view)
        {
            _view = view;
            _position = new MockTrackingPoint((MockTextSnapshot)_view.TextBuffer.CurrentSnapshot, 0);
        }

        public double Bottom
        {
            get { throw new System.NotImplementedException(); }
        }

        public Microsoft.VisualStudio.Text.Formatting.ITextViewLine ContainingTextViewLine
        {
            get { throw new System.NotImplementedException(); }
        }

        public void EnsureVisible()
        {
        }

        public double Height
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool InVirtualSpace
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool IsHidden
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public double Left
        {
            get { throw new System.NotImplementedException(); }
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.VirtualSnapshotPoint bufferPosition)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition, Microsoft.VisualStudio.Text.PositionAffinity caretAffinity)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.SnapshotPoint bufferPosition)
        {
            _view.Selection.Clear();
            _position = new MockTrackingPoint((MockTextSnapshot)bufferPosition.Snapshot, bufferPosition.Position);
            return Position;
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine, double xCoordinate, bool captureHorizontalPosition)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveTo(Microsoft.VisualStudio.Text.Formatting.ITextViewLine textLine, double xCoordinate)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveToNextCaretPosition()
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveToPreferredCoordinates()
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition MoveToPreviousCaretPosition()
        {
            throw new System.NotImplementedException();
        }

        public bool OverwriteMode
        {
            get { throw new System.NotImplementedException(); }
        }

        public CaretPosition Position
        {
            get
            {
                return new CaretPosition(
              new VirtualSnapshotPoint(_position.GetPoint(_view.TextBuffer.CurrentSnapshot)),
              new MockMappingPoint(_position),
              PositionAffinity.Predecessor);
            }
        }

        internal void SetPosition(SnapshotPoint position)
        {
            _position = new MockTrackingPoint((MockTextSnapshot)position.Snapshot, position.Position);
        }

        public event System.EventHandler<CaretPositionChangedEventArgs> PositionChanged
        {
            add
            {
            }
            remove
            {
            }
        }

        public double Right
        {
            get { throw new System.NotImplementedException(); }
        }

        public double Top
        {
            get { throw new System.NotImplementedException(); }
        }

        public double Width
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}

