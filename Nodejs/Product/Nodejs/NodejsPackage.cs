// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.NodejsTools.Commands;
using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.NodejsTools.Debugger.Remote;
using Microsoft.NodejsTools.Jade;
using Microsoft.NodejsTools.Options;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.ProjectWizard;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools
{
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
    [Guid(Guids.NodejsPackageString)]
    [ProvideOptionPage(typeof(NodejsGeneralOptionsPage), "Node.js Tools", "General", 114, 115, true)]
    [ProvideDebugEngine("Node.js Debugging", typeof(AD7ProgramProvider), typeof(AD7Engine), AD7Engine.DebugEngineId, setNextStatement: false, hitCountBp: true, justMyCodeStepping: false)]
    [ProvideDebugLanguage(NodejsConstants.Nodejs, Guids.NodejsDebugLanguageString, NodeExpressionEvaluatorGuid, AD7Engine.DebugEngineId)]
    [WebSiteProject("JavaScript", "JavaScript")]
    [ProvideProjectFactory(typeof(NodejsProjectFactory), null, null, null, null, ".\\NullPath", LanguageVsTemplate = NodejsConstants.Nodejs, SortPriority = 0x17)]   // outer flavor, no file extension
    [ProvideDebugPortSupplier("Node remote debugging", typeof(NodeRemoteDebugPortSupplier), NodeRemoteDebugPortSupplier.PortSupplierId)]
    [ProvideMenuResource("Menus.ctmenu", 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideLanguageTemplates("{349C5851-65DF-11DA-9384-00065B846F21}", NodejsConstants.JavaScript, Guids.NodejsPackageString, "Web", "Node.js Project Templates", "{" + Guids.NodejsBaseProjectFactoryString + "}", ".js", NodejsConstants.Nodejs, "{" + Guids.NodejsBaseProjectFactoryString + "}")]
    [ProvideProjectItem(typeof(BaseNodeProjectFactory), NodejsConstants.Nodejs, "FileTemplates\\NewItem", 0)]
    [ProvideLanguageService(typeof(JadeLanguageInfo), JadeContentTypeDefinition.JadeLanguageName, 3041, RequestStockColors = true, ShowSmartIndent = false, ShowCompletion = false, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = false, EnableAdvancedMembersOption = false, ShowDropDownOptions = false)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorLogicalView(typeof(JadeEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension)]
    [ProvideTextEditorAutomation(JadeContentTypeDefinition.JadeLanguageName, 3041, 3045, ProfileMigrationType.PassThrough)]
    [ProvideInteractiveWindow(Guids.NodejsInteractiveWindowString, Style = VsDockStyle.Linked, Orientation = ToolWindowOrientation.none, Window = ToolWindowGuids80.Outputwindow)]
    internal sealed partial class NodejsPackage : CommonPackage
    {
        internal const string NodeExpressionEvaluatorGuid = "{F16F2A71-1C45-4BAB-BECE-09D28CFDE3E6}";
        internal static NodejsPackage Instance;
        internal HashSet<ITextBuffer> ChangedBuffers = new HashSet<ITextBuffer>();

        // Hold references for the subscribed events. Otherwise the callbacks will be garbage collected
        // after the initialization
        private readonly List<EnvDTE.CommandEvents> subscribedCommandEvents = new List<EnvDTE.CommandEvents>();

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NodejsPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Debug.Assert(Instance == null, "NodejsPackage created multiple times");
            Instance = this;
        }

        public NodejsGeneralOptionsPage GeneralOptionsPage => GetDialogPage<NodejsGeneralOptionsPage>();

        public EnvDTE.DTE DTE => (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            SubscribeToVsCommandEvents(
                (int)VSConstants.VSStd97CmdID.AddNewProject,
                delegate { NewProjectFromExistingWizard.IsAddNewProjectCmd = true; },
                delegate { NewProjectFromExistingWizard.IsAddNewProjectCmd = false; }
            );

            ((IServiceContainer)this).AddService(typeof(ClipboardServiceBase), new ClipboardService(), true);

            RegisterProjectFactory(new NodejsProjectFactory(this));
            RegisterEditorFactory(new JadeEditorFactory(this));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var commands = new List<Command> {
                new OpenReplWindowCommand(),
                new ImportWizardCommand(),
            };

            try
            {
                commands.Add(new AzureExplorerAttachDebuggerCommand());
            }
            catch (NotSupportedException)
            {
            }

            RegisterCommands(commands, Guids.NodejsCmdSet);
            MakeDebuggerContextAvailable();

            // The variable is inherited by child processes backing Test Explorer, and is used in
            // the NTVS test discoverer and test executor to connect back to VS.
            Environment.SetEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable, Process.GetCurrentProcess().Id.ToString());
        }

        private void SubscribeToVsCommandEvents(
            int eventId,
            EnvDTE._dispCommandEvents_BeforeExecuteEventHandler beforeExecute = null,
            EnvDTE._dispCommandEvents_AfterExecuteEventHandler afterExecute = null)
        {
            var commandEventGuid = typeof(VSConstants.VSStd97CmdID).GUID.ToString("B");
            var targetEvent = this.DTE.Events.CommandEvents[commandEventGuid, eventId];
            if (beforeExecute != null)
            {
                targetEvent.BeforeExecute += beforeExecute;
            }
            if (afterExecute != null)
            {
                targetEvent.AfterExecute += afterExecute;
            }
            this.subscribedCommandEvents.Add(targetEvent);
        }

        public new IComponentModel ComponentModel => this.GetComponentModel();

        /// <summary>
        /// Makes the debugger context available - this enables our debugger when we're installed into
        /// a SKU which doesn't support every installed debugger.
        /// </summary>
        private void MakeDebuggerContextAvailable()
        {
            var monitorSelection = (IVsMonitorSelection)GetService(typeof(SVsShellMonitorSelection));
            var debugEngineGuid = AD7Engine.DebugEngineGuid;

            if (ErrorHandler.Succeeded(monitorSelection.GetCmdUIContextCookie(ref debugEngineGuid, out var contextCookie)))
            {
                ErrorHandler.ThrowOnFailure(monitorSelection.SetCmdUIContext(contextCookie, 1));
            }
        }

        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            if (toolWindowType == Guids.NodejsInteractiveWindow)
            {
                var replProvider = this.GetInteractiveWindowProvider();

                replProvider.CreateReplWindow(id);
                return VSConstants.S_OK;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }

        internal void OpenReplWindow(bool focus = true)
        {
            var replProvider = this.GetInteractiveWindowProvider();

            replProvider.OpenOrCreateWindow().Show(focus);
        }

        private InteractiveWindowProvider GetInteractiveWindowProvider()
        {
            var model = (IComponentModel)GetService(typeof(SComponentModel));
            return model.GetService<InteractiveWindowProvider>();
        }

        internal static bool TryGetStartupFileAndDirectory(System.IServiceProvider serviceProvider, out string fileName, out string directory)
        {
            var startupProject = GetStartupProject(serviceProvider);
            if (startupProject != null)
            {
                fileName = startupProject.GetStartupFile();
                directory = startupProject.GetWorkingDirectory();
            }
            else
            {
                var textView = CommonPackage.GetActiveTextView(serviceProvider);
                if (textView == null)
                {
                    fileName = null;
                    directory = null;
                    return false;
                }
                fileName = textView.GetFilePath();
                directory = Path.GetDirectoryName(fileName);
            }
            return true;
        }

        #endregion

        internal override VisualStudioTools.Navigation.LibraryManager CreateLibraryManager(CommonPackage package)
        {
            return new NodejsLibraryManager(this);
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(NodejsLibraryManager);
        }

        public override bool IsRecognizedFile(string filename)
        {
            var ext = Path.GetExtension(filename);

            return StringComparer.OrdinalIgnoreCase.Equals(ext, NodejsConstants.JavaScriptExtension);
        }

        internal new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        private T GetDialogPage<T>() where T : NodejsDialogPage
        {
            return (T)GetDialogPage(typeof(T));
        }
    }
}
