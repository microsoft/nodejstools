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
        private readonly IServiceProvider serviceProvider;

        private uint compId = VSConstants.VSCOOKIE_NIL;
        private IOleComponentManager compMgr;
        private EventHandler<ComponentManagerEventArgs> onIdle;

        public IdleManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private void EnsureInit()
        {
            if (this.compId == VSConstants.VSCOOKIE_NIL)
            {
                lock (this)
                {
                    if (this.compId == VSConstants.VSCOOKIE_NIL)
                    {
                        if (this.compMgr == null)
                        {
                            this.compMgr = (IOleComponentManager)this.serviceProvider.GetService(typeof(SOleComponentManager));
                            var crInfo = new OLECRINFO[1];
                            crInfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                            crInfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime;
                            crInfo[0].grfcadvf = (uint)0;
                            crInfo[0].uIdleTimeInterval = 0;
                            if (ErrorHandler.Failed(this.compMgr.FRegisterComponent(this, crInfo, out this.compId)))
                            {
                                this.compId = VSConstants.VSCOOKIE_NIL;
                            }
                        }
                    }
                }
            }
        }

        #region IOleComponent Members

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)=> 1;

        public int FDoIdle(uint grfidlef)
        {
            this.onIdle?.Invoke(this, new ComponentManagerEventArgs(this.compMgr));
            return 0;
        }

        internal event EventHandler<ComponentManagerEventArgs> OnIdle
        {
            add
            {
                EnsureInit();
                this.onIdle += value;
            }
            remove
            {
                EnsureInit();
                this.onIdle -= value;
            }
        }

        public int FPreTranslateMessage(MSG[] pMsg) => 0;

        public int FQueryTerminate(int fPromptUser) => 1;

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam) => 1;

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved) => IntPtr.Zero;

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
            if (this.compId != VSConstants.VSCOOKIE_NIL)
            {
                this.compMgr.FRevokeComponent(this.compId);
                this.compId = VSConstants.VSCOOKIE_NIL;
            }
        }
    }
}
