// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools
{
    public abstract class CommonPackage : Package, IOleComponent
    {
        private uint _componentID;
        private LibraryManager _libraryManager;
        private IOleComponentManager _compMgr;
        private static readonly object _commandsLock = new object();
        private static readonly Dictionary<Command, MenuCommand> _commands = new Dictionary<Command, MenuCommand>();

        #region Language-specific abstracts

        public abstract Type GetLibraryManagerType();
        internal abstract LibraryManager CreateLibraryManager(CommonPackage package);
        public abstract bool IsRecognizedFile(string filename);

        // TODO:
        // public abstract bool TryGetStartupFileAndDirectory(out string filename, out string dir);

        #endregion

        internal CommonPackage()
        {
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.IsTerminating)
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        Debug.Fail(
                            string.Format("An unhandled exception is about to terminate the process:\n\n{0}", ex.Message),
                            ex.ToString()
                        );
                    }
                    else
                    {
                        Debug.Fail(string.Format(
                            "An unhandled exception is about to terminate the process:\n\n{0}",
                            e.ExceptionObject
                        ));
                    }
                }
            };
#endif
        }

        internal static Dictionary<Command, MenuCommand> Commands => _commands;

        internal static object CommandsLock => _commandsLock;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this._componentID != 0)
                {
                    if (GetService(typeof(SOleComponentManager)) is IOleComponentManager mgr)
                    {
                        mgr.FRevokeComponent(this._componentID);
                    }
                    this._componentID = 0;
                }
                if (null != this._libraryManager)
                {
                    this._libraryManager.Dispose();
                    this._libraryManager = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            if (GetLibraryManagerType() == serviceType)
            {
                return this._libraryManager = CreateLibraryManager(this);
            }
            return null;
        }

        internal void RegisterCommands(IEnumerable<Command> commands, Guid cmdSet)
        {
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                lock (_commandsLock)
                {
                    foreach (var command in commands)
                    {
                        var beforeQueryStatus = command.BeforeQueryStatus;
                        var toolwndCommandID = new CommandID(cmdSet, command.CommandId);
                        var menuToolWin = new OleMenuCommand(command.DoCommand, toolwndCommandID);
                        if (beforeQueryStatus != null)
                        {
                            menuToolWin.BeforeQueryStatus += beforeQueryStatus;
                        }
                        mcs.AddCommand(menuToolWin);
                        _commands[command] = menuToolWin;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current IWpfTextView that is the active document.
        /// </summary>
        /// <returns></returns>
        public static IWpfTextView GetActiveTextView(System.IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                Debug.Assert(false, "No service provider");
                return null;
            }

            var monitorSelection = (IVsMonitorSelection)serviceProvider.GetService(typeof(SVsShellMonitorSelection));
            if (monitorSelection == null)
            {
                return null;
            }
            if (ErrorHandler.Failed(monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out var curDocument)))
            {
                // TODO: Report error
                return null;
            }

            var frame = curDocument as IVsWindowFrame;
            if (frame == null)
            {
                // TODO: Report error
                return null;
            }

            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView)))
            {
                // TODO: Report error
                return null;
            }

            if (docView is IVsCodeWindow)
            {
                if (ErrorHandler.Failed(((IVsCodeWindow)docView).GetPrimaryView(out var textView)))
                {
                    // TODO: Report error
                    return null;
                }

                var model = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
                var adapterFactory = model.GetService<IVsEditorAdaptersFactoryService>();
                var wpfTextView = adapterFactory.GetWpfTextView(textView);
                return wpfTextView;
            }
            return null;
        }

        [Obsolete("ComponentModel should be retrieved from an IServiceProvider")]
        public static IComponentModel ComponentModel => (IComponentModel)GetGlobalService(typeof(SComponentModel));

        internal static CommonProjectNode GetStartupProject(System.IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                Debug.Assert(false, "No service provider");
                return null;
            }
            var buildMgr = (IVsSolutionBuildManager)serviceProvider.GetService(typeof(IVsSolutionBuildManager));
            if (buildMgr != null && ErrorHandler.Succeeded(buildMgr.get_StartupProject(out var hierarchy)) && hierarchy != null)
            {
                return hierarchy.GetProject().GetCommonProject();
            }
            return null;
        }

        protected override void Initialize()
        {
            var container = (IServiceContainer)this;
            UIThread.EnsureService(this);
            container.AddService(GetLibraryManagerType(), this.CreateService, true);

            var componentManager = this._compMgr = (IOleComponentManager)GetService(typeof(SOleComponentManager));
            var crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 0;
            ErrorHandler.ThrowOnFailure(componentManager.FRegisterComponent(this, crinfo, out this._componentID));

            base.Initialize();
        }

        internal static void OpenWebBrowser(string url)
        {
            var uri = new Uri(url);
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
            return;
        }

        internal static void OpenVsWebBrowser(System.IServiceProvider serviceProvider, string url)
        {
            serviceProvider.GetUIThread().Invoke(() =>
            {
                var web = serviceProvider.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
                if (web == null)
                {
                    OpenWebBrowser(url);
                    return;
                }

                ErrorHandler.ThrowOnFailure(web.Navigate(url, (uint)__VSWBNAVIGATEFLAGS.VSNWB_ForceNew, out var frame));
                frame.Show();
            });
        }

        #region IOleComponent Members

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public virtual int FDoIdle(uint grfidlef)
        {
            if (null != this._libraryManager)
            {
                this._libraryManager.OnIdle(this._compMgr);
            }

            var onIdle = OnIdle;
            if (onIdle != null)
            {
                onIdle(this, new ComponentManagerEventArgs(this._compMgr));
            }

            return 0;
        }

        internal event EventHandler<ComponentManagerEventArgs> OnIdle;

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
    }

    internal class ComponentManagerEventArgs : EventArgs
    {
        private readonly IOleComponentManager _compMgr;

        public ComponentManagerEventArgs(IOleComponentManager compMgr)
        {
            this._compMgr = compMgr;
        }

        public IOleComponentManager ComponentManager => this._compMgr;
    }
}
