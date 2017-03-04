// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace TestUtilities.Mocks
{
    public class MockTextViewModel : ITextViewModel
    {
        public ITextBuffer DataBuffer { get; set; }

        public ITextDataModel DataModel
        {
            get { throw new NotImplementedException(); }
        }

        public ITextBuffer EditBuffer { get; set; }

        public SnapshotPoint GetNearestPointInVisualBuffer(SnapshotPoint editBufferPoint)
        {
            throw new NotImplementedException();
        }

        public SnapshotPoint GetNearestPointInVisualSnapshot(SnapshotPoint editBufferPoint, ITextSnapshot targetVisualSnapshot, PointTrackingMode trackingMode)
        {
            throw new NotImplementedException();
        }

        public bool IsPointInVisualBuffer(SnapshotPoint editBufferPoint, PositionAffinity affinity)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer VisualBuffer
        {
            get { throw new NotImplementedException(); }
        }

        public PropertyCollection Properties
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

