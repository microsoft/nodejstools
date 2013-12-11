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

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools {
    /// <summary>
    /// Custom tracking span to track the bounds of the user's code that is surrounded by
    /// the a helper function and reference tags.
    /// 
    /// The surrounding code can currently grow only from the beginning.  The user code can
    /// obviously have arbitrary edits.  We just need to remember the start of the user's
    /// code based upon edits which happen in the beginning code (which changes when reference
    /// tags in the users code change).
    /// </summary>
    class CustomTrackingSpan : ITrackingSpan {
        private readonly ITextBuffer _buffer;
        private ITextVersion _cachedVersion;
        private int _currentStart;

        public CustomTrackingSpan(ITextSnapshot snapshot, NodejsProjectionBuffer buffer) {
            _buffer = snapshot.TextBuffer;
            _cachedVersion = snapshot.Version;
            _currentStart = buffer.LeadingText.Length;
        }

        #region ITrackingSpan Members

        public SnapshotPoint GetEndPoint(ITextSnapshot snapshot) {
            return GetSpan(snapshot).End;
        }

        public Span GetSpan(ITextVersion version) {
            if (version != _cachedVersion) {
                int curStart = _currentStart;
                if (version.VersionNumber > _cachedVersion.VersionNumber) {
                    // roll forward
                    for (var currentVersion = _cachedVersion;
                        currentVersion != version;
                        currentVersion = currentVersion.Next) {

                        foreach (ITextChange change in currentVersion.Changes) {
                            if (change.OldPosition < curStart) {
                                curStart += change.Delta;
                            }
                        }
                    }
                } else {
                    // roll back
                    List<ITextVersion> versions = new List<ITextVersion>();
                    for (var currentVersion = _cachedVersion;
                        version != currentVersion;
                        currentVersion = currentVersion.Next) {
                            versions.Add(currentVersion);
                    }

                    foreach (var curVersion in versions) {
                        foreach (ITextChange change in curVersion.Changes) {
                            if (change.OldPosition < curStart) {
                                curStart -= change.Delta;
                            }
                        }
                    }
                }
                _currentStart = curStart;
                _cachedVersion = version;
            }
            
            return Span.FromBounds(
                _currentStart,
                GetEndPositionForVersion(version)
            );
        }

        private static int GetEndPositionForVersion(ITextVersion version) {
            return version.Length - NodejsProjectionBuffer.TrailingText.Length;
        }

        public SnapshotSpan GetSpan(ITextSnapshot snapshot) {
            return new SnapshotSpan(
                snapshot,
                GetSpan(snapshot.Version)
            );
        }

        public SnapshotPoint GetStartPoint(ITextSnapshot snapshot) {
            return GetSpan(snapshot).Start;
        }

        public string GetText(ITextSnapshot snapshot) {
            return GetSpan(snapshot).GetText();
        }

        public ITextBuffer TextBuffer {
            get { return _buffer; }
        }

        public TrackingFidelityMode TrackingFidelity {
            get { return TrackingFidelityMode.Forward; }
        }

        public SpanTrackingMode TrackingMode {
            get { return SpanTrackingMode.Custom; }
        }

        #endregion
    }
}
