// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl
{
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    /// <summary>
    /// This is a custom span which is like an EdgeInclusive span.  We need a custom span because elision buffers
    /// do not allow EdgeInclusive unless it spans the entire buffer.  We create snippets of our language spans
    /// and these are initially zero length.  When we insert at the beginning of these we'll end up keeping the
    /// span zero length if we're just EdgePostivie tracking.
    /// </summary>
    internal class CustomTrackingSpan : ITrackingSpan
    {
        private readonly ITrackingPoint _start, _end;
        private readonly ITextBuffer _buffer;

        public CustomTrackingSpan(ITextSnapshot snapshot, Span span, PointTrackingMode startTrackingMode, PointTrackingMode endTrackingMode)
        {
            _buffer = snapshot.TextBuffer;
            _start = snapshot.CreateTrackingPoint(span.Start, startTrackingMode);
            _end = snapshot.CreateTrackingPoint(span.End, endTrackingMode);
        }

        #region ITrackingSpan Members

        public SnapshotPoint GetEndPoint(ITextSnapshot snapshot)
        {
            return _end.GetPoint(snapshot);
        }

        public Span GetSpan(ITextVersion version)
        {
            return Span.FromBounds(
                _start.GetPosition(version),
                _end.GetPosition(version)
            );
        }

        public SnapshotSpan GetSpan(ITextSnapshot snapshot)
        {
            return new SnapshotSpan(
                snapshot,
                Span.FromBounds(_start.GetPoint(snapshot), _end.GetPoint(snapshot))
            );
        }

        public SnapshotPoint GetStartPoint(ITextSnapshot snapshot)
        {
            return _start.GetPoint(snapshot);
        }

        public string GetText(ITextSnapshot snapshot)
        {
            return GetSpan(snapshot).GetText();
        }

        public ITextBuffer TextBuffer
        {
            get { return _buffer; }
        }

        public TrackingFidelityMode TrackingFidelity
        {
            get { return TrackingFidelityMode.Forward; }
        }

        public SpanTrackingMode TrackingMode
        {
            get { return SpanTrackingMode.Custom; }
        }

        #endregion

        public override string ToString()
        {
            return "CustomSpan: " + GetSpan(_buffer.CurrentSnapshot).ToString();
        }
    }
}

