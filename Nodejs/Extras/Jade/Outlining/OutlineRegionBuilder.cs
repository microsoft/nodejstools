// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    internal abstract class OutlineRegionBuilder : IDisposable
    {
        public EventHandler<OutlineRegionsChangedEventArgs> RegionsChanged;

        protected OutlineRegionCollection CurrentRegions { get; set; }
        protected IdleTimeAsyncTask BackgroundTask { get; set; }
        protected ITextBuffer TextBuffer { get; set; }

        private long _disposed = 0;
        private object _regionsLock = new object();

        protected OutlineRegionBuilder(ITextBuffer textBuffer)
        {
            this.CurrentRegions = new OutlineRegionCollection(0);

            this.TextBuffer = textBuffer;
            this.TextBuffer.Changed += this.OnTextBufferChanged;

            this.BackgroundTask = new IdleTimeAsyncTask(this.TaskAction, this.MainThreadAction);
            this.BackgroundTask.DoTaskOnIdle(300);
        }

        protected virtual void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // In order to provide nicer experience when user presser and holds
            // ENTER or DELETE or just types really fast, we are going to track
            // regions optimistically and report changes without going through
            // async or idle processing. Idle/async is still going to hit later.

            if (e.Changes.Count > 0)
            {
                TextUtility.CombineChanges(e, out var start, out var oldLength, out var newLength);

                var changeStart = int.MaxValue;
                var changeEnd = 0;

                lock (this._regionsLock)
                {
                    // Remove affected regions and shift the remaining ones. Outlining 
                    // regions are not sorted and can overlap. Hence linear search.

                    for (var i = 0; i < this.CurrentRegions.Count; i++)
                    {
                        var region = this.CurrentRegions[i];

                        if (region.End <= start)
                        {
                            continue;
                        }

                        if (region.Contains(start) && region.Contains(start + oldLength))
                        {
                            region.Expand(0, newLength - oldLength);
                        }
                        else if (region.Start >= start + oldLength)
                        {
                            region.Shift(newLength - oldLength);
                        }
                        else
                        {
                            this.CurrentRegions.RemoveAt(i);
                            i--;
                        }

                        changeStart = Math.Min(changeStart, region.Start);
                        changeEnd = Math.Max(changeEnd, region.End);
                    }

                    if (changeStart < int.MaxValue)
                    {
                        this.CurrentRegions.TextBufferVersion = this.TextBuffer.CurrentSnapshot.Version.VersionNumber;
                    }
                }

                if (changeStart < int.MaxValue)
                {
                    if (this.RegionsChanged != null)
                    {
                        this.RegionsChanged(this, new OutlineRegionsChangedEventArgs(this.CurrentRegions, TextRange.FromBounds(changeStart, changeEnd)));
                    }
                }
            }
        }

        protected abstract void BuildRegions(OutlineRegionCollection newRegions);

        protected bool IsDisposed()
        {
            return Interlocked.Read(ref this._disposed) > 0;
        }

        protected virtual object TaskAction()
        {
            if (!IsDisposed())
            {
                var snapshot = this.TextBuffer.CurrentSnapshot;
                var newRegions = new OutlineRegionCollection(snapshot.Version.VersionNumber);

                BuildRegions(newRegions);

                lock (this._regionsLock)
                {
                    var changedRange = CompareRegions(newRegions, this.CurrentRegions, snapshot.Length);
                    return new OutlineRegionsChange(changedRange, newRegions);
                }
            }

            return null;
        }

        protected virtual void MainThreadAction(object backgroundProcessingResult)
        {
            if (!IsDisposed())
            {

                if (backgroundProcessingResult is OutlineRegionsChange result && TextRange.IsValid(result.ChangedRange))
                {
                    lock (this._regionsLock)
                    {
                        this.CurrentRegions = result.NewRegions;
                    }

                    if (this.RegionsChanged != null)
                    {
                        this.RegionsChanged(this,
                            new OutlineRegionsChangedEventArgs(this.CurrentRegions.Clone() as OutlineRegionCollection,
                            result.ChangedRange)
                         );
                    }
                }
            }
        }

        protected static ITextRange CompareRegions(
            OutlineRegionCollection newRegions,
            OutlineRegionCollection oldRegions, int upperBound)
        {
            TextRangeCollection<OutlineRegion> oldClone = null;
            TextRangeCollection<OutlineRegion> newClone = null;

            if (oldRegions != null)
            {
                oldClone = oldRegions.Clone() as OutlineRegionCollection;
                oldClone.Sort();
            }

            newClone = newRegions.Clone() as OutlineRegionCollection;
            newClone.Sort();

            return newClone.RangeDifference(oldClone, 0, upperBound);
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Read(ref this._disposed) == 0)
            {
                Interlocked.Exchange(ref this._disposed, 1);
                if (this.TextBuffer != null)
                {
                    this.TextBuffer.Changed -= this.OnTextBufferChanged;
                    this.TextBuffer = null;
                }

                if (this.BackgroundTask != null)
                {
                    this.BackgroundTask.Dispose();
                    this.BackgroundTask = null;
                }
            }
        }
        #endregion
    }
}
