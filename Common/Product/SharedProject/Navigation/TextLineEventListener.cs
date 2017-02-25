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

using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal class TextLineEventListener : IVsTextLinesEvents, IDisposable
    {
        private const int _defaultDelay = 2000;
        private string _fileName;
        private ModuleId _fileId;
        private IVsTextLines _buffer;
        private bool _isDirty;
        private IConnectionPoint _connectionPoint;
        private uint _connectionCookie;

        public TextLineEventListener(IVsTextLines buffer, string fileName, ModuleId id)
        {
            this._buffer = buffer;
            this._fileId = id;
            this._fileName = fileName;
            IConnectionPointContainer container = buffer as IConnectionPointContainer;
            if (null != container)
            {
                Guid eventsGuid = typeof(IVsTextLinesEvents).GUID;
                container.FindConnectionPoint(ref eventsGuid, out this._connectionPoint);
                this._connectionPoint.Advise(this as IVsTextLinesEvents, out this._connectionCookie);
            }
        }

        #region Properties
        public ModuleId FileID
        {
            get { return this._fileId; }
        }
        public string FileName
        {
            get { return this._fileName; }
            set { this._fileName = value; }
        }
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
            TextLineChangeEvent eh = OnFileChangedImmediate;
            if (null != eh)
            {
                eh(this, pTextLineChange, fLast);
            }

            this._isDirty = true;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if ((null != this._connectionPoint) && (0 != this._connectionCookie))
            {
                this._connectionPoint.Unadvise(this._connectionCookie);
            }
            this._connectionCookie = 0;
            this._connectionPoint = null;

            this._buffer = null;
            this._fileId = null;
        }
        #endregion

        #region Idle time processing
        public void OnIdle()
        {
            if (!this._isDirty)
            {
                return;
            }
            var onFileChanged = OnFileChanged;
            if (null != onFileChanged)
            {
                HierarchyEventArgs args = new HierarchyEventArgs(this._fileId.ItemID, this._fileName);
                args.TextBuffer = this._buffer;
                onFileChanged(this._fileId.Hierarchy, args);
            }

            this._isDirty = false;
        }
        #endregion
    }
}
