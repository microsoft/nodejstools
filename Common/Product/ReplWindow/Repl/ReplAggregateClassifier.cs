// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Projection;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl
{
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    internal class ReplAggregateClassifier : IClassifier
    {
        private readonly List<ElisionInfo> _elisionBuffers = new List<ElisionInfo>();                    // the ellison buffers we've created, 1 for each language span
        private readonly ITextBuffer _primaryBuffer;
        private readonly IBufferGraphFactoryService _bufGraphFact;
        private IBufferGraph _bufferGraph;

        public ReplAggregateClassifier(IBufferGraphFactoryService bufferGraphFactory, ITextBuffer buffer)
        {
            _primaryBuffer = buffer;
            _bufGraphFact = bufferGraphFactory;
            _bufferGraph = bufferGraphFactory.CreateBufferGraph(buffer);
        }

        #region IClassifier Members

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            List<ClassificationSpan> res = new List<ClassificationSpan>();
            foreach (var info in _elisionBuffers)
            {
                foreach (var targetSpan in info.BufferGraph.MapDownToBuffer(span, SpanTrackingMode.EdgeExclusive, info.Buffer))
                {
                    foreach (var classSpan in info.Classifier.GetClassificationSpans(targetSpan))
                    {
                        foreach (var mappedBack in info.BufferGraph.MapUpToBuffer(classSpan.Span, SpanTrackingMode.EdgeExclusive, _primaryBuffer))
                        {
                            res.Add(
                                new ClassificationSpan(
                                    mappedBack,
                                    classSpan.ClassificationType
                                )
                            );
                        }
                    }
                }
            }

            return res;
        }

        #endregion

        public void AddClassifier(IProjectionBuffer projectionBuffer, ITextBuffer textBuffer, IClassifier classifer)
        {
            var elisionInfo = new ElisionInfo(textBuffer, classifer, _bufGraphFact.CreateBufferGraph(projectionBuffer));
            _elisionBuffers.Add(elisionInfo);

            classifer.ClassificationChanged += (sender, args) =>
            {
                var classChanged = ClassificationChanged;
                if (classChanged != null)
                {
                    foreach (var span in elisionInfo.BufferGraph.MapDownToBuffer(args.ChangeSpan, SpanTrackingMode.EdgeExclusive, projectionBuffer))
                    {
                        classChanged(this, new ClassificationChangedEventArgs(span));
                    }
                }
            };
        }

        private class ElisionInfo
        {
            public readonly ITextBuffer Buffer;
            public readonly IClassifier Classifier;
            public readonly IBufferGraph BufferGraph;

            public ElisionInfo(ITextBuffer buffer, IClassifier classifier, IBufferGraph bufferGraph)
            {
                Buffer = buffer;
                Classifier = classifier;
                BufferGraph = bufferGraph;
            }
        }
    }
}

