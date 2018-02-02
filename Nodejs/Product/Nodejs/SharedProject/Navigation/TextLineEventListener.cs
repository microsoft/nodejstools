// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal class TextLineEventListener : IVsTextLinesEvents, IDisposable
    {
        private const int DefaultDelay = 2000;
        private ModuleId fileId;
        private IVsTextLines buffer;
        private bool isDirty;
        private IConnectionPoint connectionPoint;
        private uint connectionCookie;

        public TextLineEventListener(IVsTextLines buffer, string fileName, ModuleId id)
        {
            this.buffer = buffer;
            this.fileId = id;
            this.FileName = fileName;
            if (buffer is IConnectionPointContainer container)
            {
                var eventsGuid = typeof(IVsTextLinesEvents).GUID;
                container.FindConnectionPoint(ref eventsGuid, out this.connectionPoint);
                this.connectionPoint.Advise(this as IVsTextLinesEvents, out this.connectionCookie);
            }
        }

        #region Properties
        public ModuleId FileID => this.fileId;

        public string FileName { get; set; }
        public uint ConnectionCookie { get => this.connectionCookie; set => this.connectionCookie = value; }
        #endregion

        #region Events
        public event EventHandler<HierarchyEventArgs> OnFileChanged;

        public event TextLineChangeEvent OnFileChangedImmediate;

        #endregion

        #region IVsTextLinesEvents Members
        void IVsTextLinesEvents.OnChangeLineAttributes(int iFirstLine, int iLastLine)
        {
            // Do Nothing
        }

        void IVsTextLinesEvents.OnChangeLineText(TextLineChange[] pTextLineChange, int fLast)
        {
            OnFileChangedImmediate?.Invoke(this, pTextLineChange, fLast);

            this.isDirty = true;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if ((null != this.connectionPoint) && (0 != this.connectionCookie))
            {
                this.connectionPoint.Unadvise(this.connectionCookie);
            }
            this.connectionCookie = 0;
            this.connectionPoint = null;

            this.buffer = null;
            this.fileId = null;
        }
        #endregion

        #region Idle time processing
        public void OnIdle()
        {
            if (!this.isDirty)
            {
                return;
            }
            var onFileChanged = OnFileChanged;
            if (null != onFileChanged)
            {
                var args = new HierarchyEventArgs(this.fileId.ItemId, this.FileName);
                args.TextBuffer = this.buffer;
                onFileChanged(this.fileId.Hierarchy, args);
            }

            this.isDirty = false;
        }
        #endregion
    }
}
