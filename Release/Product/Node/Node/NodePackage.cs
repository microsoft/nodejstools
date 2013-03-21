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
using Microsoft.PythonTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Repl;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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
    [ProvideDebugLanguage("JavaScript", "{65791609-BA29-49CF-A214-DBFF8AEC3BC2}", NodeExpressionEvaluatorGuid, AD7Engine.DebugEngineId)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "EvalError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "RangeError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "ReferenceError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "SyntaxError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "TypeError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "URIError", State = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE)]
    [ProvideProjectFactory(typeof(NodeProjectFactory), "JavaScript", "Node.js Project Files (*.njsproj);*.njsproj", "njsproj", "njsproj", ".\\NullPath", LanguageVsTemplate = "JavaScript")]
    [ProvideDebugPortSupplier("Node remote debugging", typeof(NodeRemoteDebugPortSupplier), NodeRemoteDebugPortSupplier.PortSupplierId)]
    [ProvideMenuResource(1000, 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideEditorExtension2(typeof(NodejsEditorFactory), NodeJsFileType, 50, "*:1", ProjectGuid = "{78D985FC-2CA0-4D08-9B6B-35ACD5E5294A}", NameResourceID = 102, DefaultName = "server")]
    [ProvideEditorExtension2(typeof(NodejsEditorFactoryPromptForEncoding), NodeJsFileType, 50, "*:1", ProjectGuid = "{78D985FC-2CA0-4D08-9B6B-35ACD5E5294A}", NameResourceID = 113, DefaultName = "server")]
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

        public IContentType ContentType {
            get {
                if (_contentType == null) {
                    _contentType = ComponentModel.GetService<IContentTypeRegistryService>().GetContentType("JavaScript");
                }
                return _contentType;
            }
        }

        #endregion

        internal override PythonTools.Navigation.LibraryManager CreateLibraryManager(CommonPackage package) {
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
