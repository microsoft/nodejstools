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

using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.NodejsTools.Debugger.Remote;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.NodejsTools {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidNodePkgString)]
    [ProvideDebugEngine("Node.js Debugging", typeof(AD7ProgramProvider), typeof(AD7Engine), AD7Engine.DebugEngineId)]
    [ProvideDebugLanguage(NodeConstants.JavaScript, "{65791609-BA29-49CF-A214-DBFF8AEC3BC2}", NodeExpressionEvaluatorGuid, AD7Engine.DebugEngineId)]
    // Keep declared exceptions in sync with those given default values in NodeDebugger.GetDefaultExceptionTreatments()
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOENT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EACCES)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EADDRINUSE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EADDRNOTAVAIL)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EAFNOSUPPORT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EAGAIN)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EWOULDBLOCK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EALREADY)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EBADF)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EBADMSG)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EBUSY)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECANCELED)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECHILD)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECONNABORTED)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECONNREFUSED)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECONNRESET)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EDEADLK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EDESTADDRREQ)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EDOM)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EEXIST)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EFAULT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EFBIG)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EHOSTUNREACH)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EIDRM)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EILSEQ)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EINPROGRESS)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EINTR)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EINVAL)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EIO)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EISCONN)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EISDIR)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ELOOP)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EMFILE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EMLINK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EMSGSIZE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENAMETOOLONG)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENETDOWN)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENETRESET)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENETUNREACH)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENFILE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOBUFS)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENODATA)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENODEV)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOENT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOEXEC)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOLINK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOLCK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOMEM)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOMSG)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOPROTOOPT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSPC)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSR)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSTR)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSYS)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTCONN)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTDIR)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTEMPTY)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTSOCK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTSUP)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTTY)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENXIO)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EOVERFLOW)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPERM)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPIPE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPROTO)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPROTONOSUPPORT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPROTOTYPE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ERANGE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EROFS)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ESPIPE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ESRCH)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ETIME)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ETIMEDOUT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ETXTBSY)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EXDEV)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGHUP)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGINT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGILL)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGABRT)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGFPE)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGKILL)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGSEGV)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGTERM)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGBREAK)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGWINCH)", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "EvalError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "RangeError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "ReferenceError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "SyntaxError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "TypeError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "URIError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideProjectFactory(typeof(NodeProjectFactory), NodeConstants.Nodejs, "Node.js Project Files (*.njsproj);*.njsproj", "njsproj", "njsproj", ".\\NullPath", LanguageVsTemplate = NodeConstants.Nodejs)]
    [ProvideDebugPortSupplier("Node remote debugging", typeof(NodeRemoteDebugPortSupplier), NodeRemoteDebugPortSupplier.PortSupplierId)]
    [ProvideMenuResource(1000, 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideEditorExtension2(typeof(NodejsEditorFactory), NodeJsFileType, 50, "*:1", ProjectGuid = "{78D985FC-2CA0-4D08-9B6B-35ACD5E5294A}", NameResourceID = 102, DefaultName = "server", TemplateDir="FileTemplates\\NewItem")]
    [ProvideEditorExtension2(typeof(NodejsEditorFactoryPromptForEncoding), NodeJsFileType, 50, "*:1", ProjectGuid = "{78D985FC-2CA0-4D08-9B6B-35ACD5E5294A}", NameResourceID = 113, DefaultName = "server")]
    [ProvideProjectItem(typeof(BaseNodeProjectFactory), NodeConstants.Nodejs, "FileTemplates\\NewItem", 0)]
    [ProvideLanguageTemplates("{349C5851-65DF-11DA-9384-00065B846F21}", NodeConstants.Nodejs, GuidList.guidNodePkgString, "Web", "Node.js Project Templates", "{" + BaseNodeProjectFactory.BaseNodeProjectGuid + "}", ".js", NodeConstants.Nodejs, "{" + BaseNodeProjectFactory.BaseNodeProjectGuid + "}")]
    public sealed class NodePackage : CommonPackage {
        internal const string NodeExpressionEvaluatorGuid = "{F16F2A71-1C45-4BAB-BECE-09D28CFDE3E6}";
        private IContentType _contentType;
        internal const string NodeJsFileType = ".njs";
        internal static readonly Guid _jsLangSvcGuid = new Guid("{71d61d27-9011-4b17-9469-d20f798fb5c0}");

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NodePackage() {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            RegisterProjectFactory(new NodeProjectFactory(this));
            RegisterEditorFactory(new NodejsEditorFactory(this));
            RegisterEditorFactory(new NodejsEditorFactoryPromptForEncoding(this));

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            CommandID replWindowCmdId = new CommandID(GuidList.guidNodeCmdSet, PkgCmdId.cmdidReplWindow);
            MenuCommand replWindowCmd = new MenuCommand(OpenReplWindow, replWindowCmdId);
            mcs.AddCommand(replWindowCmd);

            CommandID openRemoteDebugProxyFolderCmdId = new CommandID(GuidList.guidNodeCmdSet, PkgCmdId.cmdidOpenRemoteDebugProxyFolder);
            MenuCommand openRemoteDebugProxyFolderCmd = new MenuCommand(OpenRemoteDebugProxyFolder, openRemoteDebugProxyFolderCmdId);
            mcs.AddCommand(openRemoteDebugProxyFolderCmd);
        }

        private void OpenReplWindow(object sender, EventArgs args) {
            var compModel = ComponentModel;
            var provider = compModel.GetService<IReplWindowProvider>();

            var window = provider.FindReplWindow(NodeReplEvaluatorProvider.NodeReplId);
            if (window == null) {
                window = provider.CreateReplWindow(
                    ContentType,
                    "Node.js Interactive Window",
                    _jsLangSvcGuid,
                    NodeReplEvaluatorProvider.NodeReplId
                );
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)((ToolWindowPane)window).Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            ((IReplWindow)window).Focus();
        }

        private void OpenRemoteDebugProxyFolder(object sender, EventArgs args) {
            // Open explorer to folder
            if (!File.Exists(RemoteDebugProxyFolder)) {
                MessageBox.Show(String.Format("Remote Debug Proxy \"{0}\" does not exist.", RemoteDebugProxyFolder), "Node.js Tools for Visual Studio");
            } else {
                Process.Start("explorer", string.Format("/e,/select,{0}", RemoteDebugProxyFolder));
            }
        }

        private static string remoteDebugProxyFolder = null;
        public static string RemoteDebugProxyFolder {
            get {
                // Lazily evaluated
                if (remoteDebugProxyFolder != null) {
                    return remoteDebugProxyFolder;
                }

                // Try HKCU
                try {
                    using (
                        RegistryKey software = Registry.CurrentUser.OpenSubKey("Software"),
                        microsoft = software.OpenSubKey("Microsoft"),
                        node = microsoft.OpenSubKey("NodeJSTools")
                    ) {
                        if (node != null) {
                            remoteDebugProxyFolder = (string)node.GetValue("RemoteDebugProxyScript");
                        }
                    }
                } catch (Exception) {
                }

                // Try HKLM
                if (remoteDebugProxyFolder == null) {
                    try {
                        using (
                            RegistryKey software = Registry.LocalMachine.OpenSubKey("Software"),
                            microsoft = software.OpenSubKey("Microsoft"),
                            node = microsoft.OpenSubKey("NodeJSTools")
                        ) {
                            if (node != null) {
                                remoteDebugProxyFolder = (string)node.GetValue("RemoteDebugProxyScript");
                            }
                        }
                    } catch (Exception) {
                    }
                }

                return remoteDebugProxyFolder;
            }
        }

        public IContentType ContentType {
            get {
                if (_contentType == null) {
                    _contentType = ComponentModel.GetService<IContentTypeRegistryService>().GetContentType(NodeConstants.JavaScript);
                }
                return _contentType;
            }
        }

        #endregion

        internal override VisualStudioTools.Navigation.LibraryManager CreateLibraryManager(CommonPackage package) {
            return new NodeLibraryManager(this);
        }

        public override Type GetLibraryManagerType() {
            return typeof(NodeLibraryManager);
        }

        public override bool IsRecognizedFile(string filename) {
            var ext = Path.GetExtension(filename);

            return String.Equals(ext, NodeConstants.FileExtension, StringComparison.OrdinalIgnoreCase);
        }

        internal new object GetService(Type serviceType) {
            return base.GetService(serviceType);
        }

        private static string nodePath = null;

        public static string NodePath {
            get {
                if (nodePath != null)
                    return nodePath;
                //Fall back to a well known location if lookup fails
                string installPath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "nodejs");
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Node.js"))
                    {
                        if (key != null)
                        {
                            string keyValue = (string)key.GetValue("InstallPath", installPath);
                            installPath = String.IsNullOrEmpty(keyValue) ? installPath : keyValue;
                        }
                    }                    
                }
                catch (Exception)
                {
                }
                nodePath = System.IO.Path.Combine(installPath, "node.exe");
                return nodePath;
            }
        }

#if UNIT_TEST_INTEGRATION
        // var testCase = require('./test/test-doubled.js'); for(var x in testCase) { console.log(x); }
        public static string EvaluateJavaScript(string code) {
            // TODO: Escaping code
            string args = "-e \"" + code + "\"";
            var psi = new ProcessStartInfo(NodePath, args);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            var proc = Process.Start(psi);
            var outpReceiver = new OutputReceiver();
            proc.OutputDataReceived += outpReceiver.DataRead;
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();

            return outpReceiver._data.ToString();
        }

        private void GetTestCases(string module) {
            var testCases = EvaluateJavaScript(
                String.Format("var testCase = require('{0}'); for(var x in testCase) { console.log(x); }", module));
            foreach (var testCase in testCases.Split(new[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
            }
        }

        class OutputReceiver {
            internal readonly StringBuilder _data = new StringBuilder();
            
            public void DataRead(object sender, DataReceivedEventArgs e) {
                if (e.Data != null) {
                    _data.Append(e.Data);
                }
            }
        }
#endif
    }
}
