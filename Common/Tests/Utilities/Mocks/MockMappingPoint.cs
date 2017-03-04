// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace TestUtilities.Mocks
{
    public class MockMappingPoint : IMappingPoint
    {
        private readonly ITrackingPoint _trackingPoint;

        public MockMappingPoint(ITrackingPoint trackingPoint)
        {
            _trackingPoint = trackingPoint;
        }

        public ITextBuffer AnchorBuffer
        {
            get { throw new NotImplementedException(); }
        }

        public IBufferGraph BufferGraph
        {
            get { throw new NotImplementedException(); }
        }

        public SnapshotPoint? GetInsertionPoint(Predicate<ITextBuffer> match)
        {
            throw new NotImplementedException();
        }

        public SnapshotPoint? GetPoint(Predicate<ITextBuffer> match, PositionAffinity affinity)
        {
            throw new NotImplementedException();
        }

        public SnapshotPoint? GetPoint(ITextSnapshot targetSnapshot, PositionAffinity affinity)
        {
            try
            {
                return _trackingPoint.GetPoint(targetSnapshot);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public SnapshotPoint? GetPoint(ITextBuffer targetBuffer, PositionAffinity affinity)
        {
            return GetPoint(targetBuffer.CurrentSnapshot, affinity);
        }
    }
}

