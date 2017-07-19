// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools
{
    using IServiceProvider = System.IServiceProvider;

    /// <summary>
    /// Provides access to Visual Studio's idle processing using a simple .NET event
    /// based API.
    /// 
    /// The IdleManager in instantiated with an IServiceProvider and then the OnIdle
    /// event can be hooked or disconnected as needed.
    /// 
    /// Disposing of the IdleManager will disconnect from Visual Studio idle processing.
    /// </summary>
    internal sealed class IdleManager : IOleComponent, IDisposable
    {
        private uint _compId = VSConstants.VSCOOKIE_NIL;
        private readonly IServiceProvider _serviceProvider;
        private IOleComponentManager _compMgr;
        private EventHandler<ComponentManagerEventArgs> _onIdle;

        public IdleManager(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        private void EnsureInit()
        {
            if (this._compId == VSConstants.VSCOOKIE_NIL)
            {
                lock (this)
                {
                    if (this._compId == VSConstants.VSCOOKIE_NIL)
                    {
                        if (this._compMgr == null)
                        {
                            this._compMgr = (IOleComponentManager)this._serviceProvider.GetService(typeof(SOleComponentManager));
                            var crInfo = new OLECRINFO[1];
                            crInfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                            crInfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime;
                            crInfo[0].grfcadvf = (uint)0;
                            crInfo[0].uIdleTimeInterval = 0;
                            if (ErrorHandler.Failed(this._compMgr.FRegisterComponent(this, crInfo, out this._compId)))
                            {
                                this._compId = VSConstants.VSCOOKIE_NIL;
                            }
                        }
                    }
                }
            }
        }

        #region IOleComponent Members

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FDoIdle(uint grfidlef)
        {
            var onIdle = this._onIdle;
            if (onIdle != null)
            {
                onIdle(this, new ComponentManagerEventArgs(this._compMgr));
            }

            return 0;
        }

        internal event EventHandler<ComponentManagerEventArgs> OnIdle
        {
            add
            {
                EnsureInit();
                this._onIdle += value;
            }
            remove
            {
                EnsureInit();
                this._onIdle -= value;
            }
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }

        #endregion

        public void Dispose()
        {
            if (this._compId != VSConstants.VSCOOKIE_NIL)
            {
                this._compMgr.FRevokeComponent(this._compId);
                this._compId = VSConstants.VSCOOKIE_NIL;
            }
        }
    }
}
