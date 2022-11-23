// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.NodejsTools.Commands;
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
    [Guid(LegacyPackage.LegacyPackageString)]
    [ProvideLanguageService(typeof(JadeLanguageInfo), JadeContentTypeDefinition.JadeLanguageName, 3041, RequestStockColors = true, ShowSmartIndent = false, ShowCompletion = false, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = false, EnableAdvancedMembersOption = false, ShowDropDownOptions = false)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorLogicalView(typeof(JadeEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension)]
    [ProvideTextEditorAutomation(JadeContentTypeDefinition.JadeLanguageName, 3041, 3045, ProfileMigrationType.PassThrough)]
    [ProvideInteractiveWindow(Guids.NodejsInteractiveWindowString, Style = VsDockStyle.Linked, Orientation = ToolWindowOrientation.none, Window = ToolWindowGuids80.Outputwindow)]
    internal sealed partial class LegacyPackage : CommonPackage
    {
        internal const string LegacyPackageString = "{041992A4-AF50-4FAA-B2B0-60E3C0E0BBCF}";

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
        public LegacyPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Debug.Assert(Instance == null, "LegacyPackage created multiple times");
            Instance = this;
        }

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
            
            RegisterEditorFactory(new JadeEditorFactory(this));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var commands = new List<Command> {
                new OpenReplWindowCommand(),
            };            

            RegisterCommands(commands, Guids.NodejsCmdSet);

            // The variable is inherited by child processes backing Test Explorer, and is used in
            // the NTVS test discoverer and test executor to connect back to VS.
            Environment.SetEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable, Process.GetCurrentProcess().Id.ToString());
        }


        public new IComponentModel ComponentModel => this.GetComponentModel();

        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            if (toolWindowType == Guids.NodejsInteractiveWindow)
            {
                var replProvider = this.GetInteractiveWindowProvider();

                replProvider.OpenOrCreateWindow(id);
                return VSConstants.S_OK;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }

        internal void OpenReplWindow(bool focus = true)
        {
           var replProvider = this.GetInteractiveWindowProvider();

           replProvider.OpenOrCreateWindow(-1).Show(focus);
        }

        private InteractiveWindowProvider GetInteractiveWindowProvider()
        {
           var model = (IComponentModel)GetService(typeof(SComponentModel));
           return model.GetService<InteractiveWindowProvider>();
        }

        internal new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }
    }
}
