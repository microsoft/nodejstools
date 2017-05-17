// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.NodejsTools.Repl
{
    internal class InlineReplAdornmentManager : ITagger<IntraTextAdornmentTag>
    {
        private readonly ITextView _textView;
        private readonly List<(SnapshotPoint, ZoomableInlineAdornment)> _tags;
        private readonly Dispatcher _dispatcher;

        internal InlineReplAdornmentManager(ITextView textView)
        {
            _textView = textView;
            _tags = new List<(SnapshotPoint, ZoomableInlineAdornment)>();
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var result = new List<TagSpan<IntraTextAdornmentTag>>();
            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i].Item1.Snapshot != _textView.TextSnapshot)
                {
                    // update to the latest snapshot
                    _tags[i] = (
                        _tags[i].Item1.TranslateTo(_textView.TextSnapshot, PointTrackingMode.Negative),
                        _tags[i].Item2);
                }

                var span = new SnapshotSpan(_textView.TextSnapshot, _tags[i].Item1, 0);
                bool intersects = false;
                foreach (var applicableSpan in spans)
                {
                    if (applicableSpan.TranslateTo(_textView.TextSnapshot, SpanTrackingMode.EdgeInclusive).IntersectsWith(span))
                    {
                        intersects = true;
                        break;
                    }
                }
                if (!intersects)
                {
                    continue;
                }
                var tag = new IntraTextAdornmentTag(_tags[i].Item2, null);
                result.Add(new TagSpan<IntraTextAdornmentTag>(span, tag));
            }
            return result;
        }

        public void AddAdornment(ZoomableInlineAdornment uiElement, SnapshotPoint targetLoc)
        {
            if (Dispatcher.CurrentDispatcher != _dispatcher)
            {
                _dispatcher.BeginInvoke(new Action(() => AddAdornment(uiElement, targetLoc)));
                return;
            }
            var targetLine = targetLoc.GetContainingLine();
            _tags.Add((targetLoc, uiElement));
            var handler = TagsChanged;
            if (handler != null)
            {
                var span = new SnapshotSpan(_textView.TextSnapshot, targetLine.Start, targetLine.LengthIncludingLineBreak);
                var args = new SnapshotSpanEventArgs(span);
                handler(this, args);
            }
        }

        public IList<(SnapshotPoint, ZoomableInlineAdornment)> Adornments => _tags;

        public void RemoveAll() => _tags.Clear();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
