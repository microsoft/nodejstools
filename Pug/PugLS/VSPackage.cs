//------------------------------------------------------------------------------
// <copyright file="VSPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TPL = System.Threading.Tasks;

namespace Microsoft.VisualStudio.Pug
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideLanguageService(typeof(JadeLanguageInfo), JadeContentTypeDefinition.JadeLanguageName, 3041, RequestStockColors = true, ShowSmartIndent = false, ShowCompletion = false, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = false, EnableAdvancedMembersOption = false, ShowDropDownOptions = false)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorLogicalView(typeof(JadeEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension)]
    [ProvideTextEditorAutomation(JadeContentTypeDefinition.JadeLanguageName, 3041, 3045, ProfileMigrationType.PassThrough)]
    public sealed class VSPackage : Package, IOleComponent
    {
        /// <summary>
        /// VSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "a51da627-69bb-4c8b-bf8c-dac72b6b020e";

        internal static VSPackage Instance;
        private IOleComponentManager _compMgr;
        private uint _componentID;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            Instance = this;
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            var componentManager = this._compMgr = (IOleComponentManager)GetService(typeof(SOleComponentManager));
            var crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 0;
            ErrorHandler.ThrowOnFailure(componentManager.FRegisterComponent(this, crinfo, out this._componentID));

            base.Initialize();

            RegisterEditorFactory(new JadeEditorFactory(this));
        }

        internal event EventHandler<ComponentManagerEventArgs> OnIdle;

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FDoIdle(uint grfidlef)
        {
            var onIdle = OnIdle;
            if (onIdle != null)
            {
                onIdle(this, new ComponentManagerEventArgs(this._compMgr));
            }

            return 0;
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

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this._componentID != 0)
                {
                    var mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                    if (mgr != null)
                    {
                        mgr.FRevokeComponent(this._componentID);
                    }
                    this._componentID = 0;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
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

    internal static class VsExtensions
    {

        internal static UIThreadBase GetUIThread(this System.IServiceProvider serviceProvider)
        {
            return (UIThreadBase)serviceProvider.GetService(typeof(UIThreadBase));
        }

        public static bool IsCriticalException(this Exception ex)
        {
            return ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is ThreadAbortException ||
                ex is AccessViolationException;
        }
    }

    /// <summary>
    /// Provides the ability to run code on the VS UI thread.
    /// 
    /// UIThreadBase must be an abstract class rather than an interface because the CLR
    /// doesn't take assembly names into account when generating an interfaces GUID, resulting 
    /// in resolution issues when we reference the interface from multiple assemblies.
    /// </summary>
    public abstract class UIThreadBase
    {
        public abstract void Invoke(Action action);
        public abstract T Invoke<T>(Func<T> func);
        public abstract TPL.Task InvokeAsync(Action action);
        public abstract TPL.Task<T> InvokeAsync<T>(Func<T> func);
        public abstract TPL.Task InvokeTask(Func<TPL.Task> func);
        public abstract TPL.Task<T> InvokeTask<T>(Func<TPL.Task<T>> func);
        public abstract void MustBeCalledFromUIThreadOrThrow();

        public abstract bool InvokeRequired
        {
            get;
        }
    }
}
