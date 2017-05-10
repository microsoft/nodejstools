// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Profiling
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
    [Description("Node.js Tools Profiling Package")]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0.0.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ProfilingGuids.NodejsProfilingPkgString)]
    // set the window to dock where Toolbox/Performance Explorer dock by default
    [ProvideToolWindow(typeof(PerfToolWindow), Orientation = ToolWindowOrientation.Left, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindToolbox)]
    [ProvideFileFilterAttribute("{" + ProfilingGuids.NodejsProfilingPkgString + "}", "/1", "Node.js Performance Session (*" + PerfFileType + ");*" + PerfFileType, 100)]
    [ProvideEditorExtension(typeof(ProfilingSessionEditorFactory), ".njsperf", 50,
          ProjectGuid = ProfilingGuids.NodejsProfilingPkgString,
          NameResourceID = 105,
          DefaultName = "NodejsPerfSession")]
    [ProvideAutomationObject("NodejsProfiling")]
    internal sealed class NodejsProfilingPackage : Package
    {
        internal static NodejsProfilingPackage Instance;
        private static ProfiledProcess _profilingProcess;   // process currently being profiled
        internal static string NodeProjectGuid = "{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
        internal const string PerformanceFileFilter = "Performance Report Files|*.vspx;*.vsps";
        private AutomationProfiling _profilingAutomation;
        private static OleMenuCommand _stopCommand, _startCommand, _startWizard, _startProfiling, _startCommandCtx;
        internal const string PerfFileType = ".njsperf";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NodejsProfilingPackage()
        {
            Instance = this;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            UIThread.EnsureService(this);
            var shell = (IVsShell)GetService(typeof(SVsShell));

            // we call into the Node.js package, so we need it loaded.
            // TODO - Ideally this wouldn't be hardcoded in here but we don't have a good shared location
            //    move this guid to be from a shared file
            //   
            Guid nodePackage = new Guid("FE8A8C3D-328A-476D-99F9-2A24B75F8C7F");
            IVsPackage package;
            shell.LoadPackage(ref nodePackage, out package);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidStartNodeProfiling);
                var oleMenuItem = new OleMenuCommand(StartProfilingWizard, menuCommandID);
                oleMenuItem.BeforeQueryStatus += IsProfilingActive;
                _startWizard = oleMenuItem;
                mcs.AddCommand(oleMenuItem);

                // Create the command for the menu item.
                menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidPerfExplorer);
                oleMenuItem = new OleMenuCommand(ShowPeformanceExplorer, menuCommandID);
                mcs.AddCommand(oleMenuItem);

                menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidAddPerfSession);
                oleMenuItem = new OleMenuCommand(AddPerformanceSession, menuCommandID);
                oleMenuItem.BeforeQueryStatus += IsProfilingActive;
                mcs.AddCommand(oleMenuItem);

                menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidStartProfiling);
                oleMenuItem = _startCommand = new OleMenuCommand(StartProfiling, menuCommandID);
                oleMenuItem.BeforeQueryStatus += IsProfilingActiveAndSessionsExist;
                mcs.AddCommand(oleMenuItem);

                // Exec is handled by the Performance Explorer node, but we want to handle QueryStatus here to disable
                // the command when another profiling session is running.
                menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidPerfCtxStartProfiling);
                oleMenuItem = _startCommandCtx = new OleMenuCommand(null, menuCommandID);
                oleMenuItem.BeforeQueryStatus += IsProfilingActiveAndSessionsExist;
                mcs.AddCommand(oleMenuItem);

                menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidStopProfiling);
                _stopCommand = oleMenuItem = new OleMenuCommand(StopProfiling, menuCommandID);
                oleMenuItem.BeforeQueryStatus += IsProfilingInactive;

                mcs.AddCommand(oleMenuItem);

                menuCommandID = new CommandID(ProfilingGuids.NodejsProfilingCmdSet, (int)PkgCmdIDList.cmdidStartPerformanceAnalysis);
                _startProfiling = oleMenuItem = new OleMenuCommand(StartPerfAnalysis, menuCommandID);
                oleMenuItem.BeforeQueryStatus += IsNodejsProjectStartup;
                mcs.AddCommand(oleMenuItem);
            }

            //Create Editor Factory. Note that the base Package class will call Dispose on it.
            base.RegisterEditorFactory(new ProfilingSessionEditorFactory(this));
        }

        private void StartPerfAnalysis(object sender, EventArgs e)
        {
            var view = new ProfilingTargetView();

            var sessions = ShowPerformanceExplorer().Sessions;
            SessionNode activeSession = sessions.ActiveSession;
            if (activeSession == null ||
                activeSession.Target.ProjectTarget == null ||
                !ProjectTarget.IsSame(activeSession.Target.ProjectTarget, view.Project.GetTarget()))
            {
                // need to create a new session
                var target = new ProfilingTarget() { ProjectTarget = view.Project.GetTarget() };

                activeSession = AddPerformanceSession(
                    view.Project.Name,
                    target
                );
            }

            ProfileProjectTarget(activeSession, activeSession.Target.ProjectTarget, true);
        }

        private void IsNodejsProjectStartup(object sender, EventArgs e)
        {
            bool foundStartupProject = false;
            var dteService = (EnvDTE.DTE)(GetService(typeof(EnvDTE.DTE)));

            if (dteService.Solution.SolutionBuild.StartupProjects != null)
            {
                var startupProjects = ((object[])dteService.Solution.SolutionBuild.StartupProjects).Select(x => x.ToString());
                foreach (EnvDTE.Project project in dteService.Solution.Projects)
                {
                    var kind = project.Kind;
                    if (String.Equals(kind, NodejsProfilingPackage.NodeProjectGuid, StringComparison.OrdinalIgnoreCase))
                    {
                        if (startupProjects.Contains(project.UniqueName, StringComparer.OrdinalIgnoreCase))
                        {
                            foundStartupProject = true;
                            break;
                        }
                    }
                }
            }

            var oleMenu = sender as OleMenuCommand;

            oleMenu.Enabled = foundStartupProject;
        }

        protected override object GetAutomationObject(string name)
        {
            if (name == "NodejsProfiling")
            {
                if (_profilingAutomation == null)
                {
                    var pane = (PerfToolWindow)this.FindToolWindow(typeof(PerfToolWindow), 0, true);
                    _profilingAutomation = new AutomationProfiling(pane.Sessions);
                }
                return _profilingAutomation;
            }

            return base.GetAutomationObject(name);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void StartProfilingWizard(object sender, EventArgs e)
        {
            var targetView = new ProfilingTargetView();
            var dialog = new LaunchProfiling(targetView);
            var res = dialog.ShowModal() ?? false;
            if (res && targetView.IsValid)
            {
                var target = targetView.GetTarget();
                if (target != null)
                {
                    ProfileTarget(target);
                }
            }
        }

        internal SessionNode ProfileTarget(ProfilingTarget target, bool openReport = true)
        {
            bool save;
            string name = target.GetProfilingName(out save);
            var session = ShowPerformanceExplorer().Sessions.AddTarget(target, name, save);

            StartProfiling(target, session, openReport);
            return session;
        }

        internal void StartProfiling(ProfilingTarget target, SessionNode session, bool openReport = true)
        {
            if (!Utilities.SaveDirtyFiles())
            {
                // Abort
                return;
            }

            if (target.ProjectTarget != null)
            {
                ProfileProjectTarget(session, target.ProjectTarget, openReport);
            }
            else if (target.StandaloneTarget != null)
            {
                ProfileStandaloneTarget(session, target.StandaloneTarget, openReport);
            }
            else
            {
                if (MessageBox.Show(Resources.NoProfilingConfiguredMessageText, Resources.NoProfilingConfiguredMessageCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var newTarget = session.OpenTargetProperties();
                    if (newTarget != null && (newTarget.ProjectTarget != null || newTarget.StandaloneTarget != null))
                    {
                        StartProfiling(newTarget, session, openReport);
                    }
                }
            }
        }

        private void ProfileProjectTarget(SessionNode session, ProjectTarget projectTarget, bool openReport)
        {
            var targetGuid = projectTarget.TargetProject;

            var dte = (EnvDTE.DTE)GetGlobalService(typeof(EnvDTE.DTE));
            EnvDTE.Project projectToProfile = null;
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                var kind = project.Kind;

                if (String.Equals(kind, NodejsProfilingPackage.NodeProjectGuid, StringComparison.OrdinalIgnoreCase))
                {
                    var guid = project.Properties.Item("Guid").Value as string;

                    Guid guidVal;
                    if (Guid.TryParse(guid, out guidVal) && guidVal == projectTarget.TargetProject)
                    {
                        projectToProfile = project;
                        break;
                    }
                }
            }

            if (projectToProfile != null)
            {
                var t = ProfileProject(session, projectToProfile, openReport);
            }
            else
            {
                MessageBox.Show(Resources.ProjectNotFoundErrorMessageText, Resources.NodejsToolsForVS);
            }
        }

        internal async System.Threading.Tasks.Task ProfileProject(SessionNode session, EnvDTE.Project projectToProfile, bool openReport)
        {
            var uiThread = (UIThreadBase)NodejsProfilingPackage.Instance.GetService(typeof(UIThreadBase));
            if (!await uiThread.InvokeTask(() => EnsureProjectUpToDate(projectToProfile)) &&
                await uiThread.InvokeAsync(() => MessageBox.Show(Resources.FailedToBuild, Resources.NodejsToolsForVS, MessageBoxButton.YesNo)) == MessageBoxResult.No)
            {
                return;
            }

            var interpreterArgs = (string)projectToProfile.Properties.Item(NodeProjectProperty.NodeExeArguments).Value;
            var scriptArgs = (string)projectToProfile.Properties.Item(NodeProjectProperty.ScriptArguments).Value;
            var startBrowser = (bool)projectToProfile.Properties.Item(NodeProjectProperty.StartWebBrowser).Value;
            string launchUrl = (string)projectToProfile.Properties.Item(NodeProjectProperty.LaunchUrl).Value;

            int? port = (int?)projectToProfile.Properties.Item(NodeProjectProperty.NodejsPort).Value;

            string interpreterPath = (string)projectToProfile.Properties.Item(NodeProjectProperty.NodeExePath).Value;

            string startupFile = (string)projectToProfile.Properties.Item("StartupFile").Value;
            if (String.IsNullOrEmpty(startupFile))
            {
                MessageBox.Show(Resources.NoConfiguredStatupFileErrorMessageText, Resources.NodejsToolsForVS);
                return;
            }

            string workingDir = projectToProfile.Properties.Item("WorkingDirectory").Value as string;
            if (String.IsNullOrEmpty(workingDir) || workingDir == ".")
            {
                workingDir = projectToProfile.Properties.Item("ProjectHome").Value as string;
                if (String.IsNullOrEmpty(workingDir))
                {
                    workingDir = Path.GetDirectoryName(projectToProfile.FullName);
                }
            }

            RunProfiler(
                session,
                interpreterPath,
                interpreterArgs,
                startupFile,
                scriptArgs,
                workingDir,
                null,
                openReport,
                launchUrl,
                port,
                startBrowser
            );
        }

        private class UpdateSolutionEvents : IVsUpdateSolutionEvents
        {
            private readonly TaskCompletionSource<bool> SuccessSource = new TaskCompletionSource<bool>();

            public Task<bool> Task
            {
                get
                {
                    return SuccessSource.Task;
                }
            }

            public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
            {
                return VSConstants.S_OK;
            }

            public int UpdateSolution_Begin(ref int pfCancelUpdate)
            {
                pfCancelUpdate = 0;
                return VSConstants.S_OK;
            }

            public int UpdateSolution_Cancel()
            {
                return VSConstants.S_OK;
            }

            public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
            {
                SuccessSource.SetResult(fSucceeded != 0);
                return VSConstants.S_OK;
            }

            public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
            {
                pfCancelUpdate = 0;
                return VSConstants.S_OK;
            }
        }

        /// <summary>
        /// Ensures the project is up to date.  Returns true if the project is up to date
        /// or is successfully built, false if it's not up to date.
        /// </summary>
        private async Task<bool> EnsureProjectUpToDate(EnvDTE.Project projectToProfile)
        {
            var guid = Guid.Parse((string)projectToProfile.Properties.Item("Guid").Value);
            var solution = (IVsSolution)GetService(typeof(SVsSolution));
            IVsHierarchy hierarchy;
            if (ErrorHandler.Succeeded(solution.GetProjectOfGuid(ref guid, out hierarchy)))
            {
                var buildMan = (IVsSolutionBuildManager)GetService(typeof(SVsSolutionBuildManager));
                if (buildMan != null)
                {
                    if (((IVsSolutionBuildManager3)buildMan).AreProjectsUpToDate(0) == VSConstants.S_OK)
                    {
                        // projects are up to date, no need to build.
                        return true;
                    }

                    uint updateCookie = VSConstants.VSCOOKIE_NIL;
                    var updateEvents = new UpdateSolutionEvents();
                    try
                    {
                        if (ErrorHandler.Succeeded(buildMan.AdviseUpdateSolutionEvents(updateEvents, out updateCookie)))
                        {
                            int hr;
                            if (ErrorHandler.Succeeded(
                                hr = buildMan.StartSimpleUpdateProjectConfiguration(
                                hierarchy,
                                null,
                                null,
                                (uint)(VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD),
                                0,
                                0
                            )))
                            {
                                return await updateEvents.Task;
                            }
                        }
                    }
                    finally
                    {
                        if (updateCookie != VSConstants.VSCOOKIE_NIL)
                        {
                            buildMan.UnadviseUpdateSolutionEvents(updateCookie);
                        }
                    }
                }
            }

            return true;
        }

        private static void ProfileStandaloneTarget(SessionNode session, StandaloneTarget runTarget, bool openReport)
        {
            RunProfiler(
                session,
                runTarget.InterpreterPath,
                String.Empty,             // interpreter args
                runTarget.Script,
                runTarget.Arguments,
                runTarget.WorkingDirectory,
                null,           // env vars
                openReport,
                null,            // launch url,
                null,            // port
                false            // start browser
            );
        }

        private static void RunProfiler(SessionNode session, string interpreter, string interpreterArgs, string script, string scriptArgs, string workingDir, Dictionary<string, string> env, bool openReport, string launchUrl, int? port, bool startBrowser)
        {
            if (String.IsNullOrWhiteSpace(interpreter))
            {
                Nodejs.ShowNodejsNotInstalled();
                return;
            }
            else if (!File.Exists(interpreter))
            {
                Nodejs.ShowNodejsPathNotFound(interpreter);
                return;
            }

            var arch = NativeMethods.GetBinaryType(interpreter);

            bool jmc = true;
            using (var vsperfKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings).OpenSubKey("VSPerf"))
            {
                if (vsperfKey != null)
                {
                    var value = vsperfKey.GetValue("tools.options.justmycode");
                    int jmcSetting;
                    if (value != null && value is string && Int32.TryParse((string)value, out jmcSetting))
                    {
                        jmc = jmcSetting != 0;
                    }
                }
            }

            var process = new ProfiledProcess(interpreter, interpreterArgs, script, scriptArgs, workingDir, env, arch, launchUrl, port, startBrowser, jmc);

            string baseName = Path.GetFileNameWithoutExtension(session.Filename);
            string date = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string outPath = Path.Combine(Path.GetDirectoryName(session.Filename), baseName + "_" + date + ".vspx");

            int count = 1;
            while (File.Exists(outPath))
            {
                outPath = Path.Combine(Path.GetTempPath(), baseName + "_" + date + "(" + count + ").vspx");
                count++;
            }

            process.ProcessExited += (sender, args) =>
            {
                var dte = (EnvDTE.DTE)NodejsProfilingPackage.GetGlobalService(typeof(EnvDTE.DTE));
                _profilingProcess = null;
                _stopCommand.Enabled = false;
                _startCommand.Enabled = true;
                _startCommandCtx.Enabled = true;
                if (openReport && File.Exists(outPath))
                {
                    dte.ItemOperations.OpenFile(outPath);
                }
            };

            session.AddProfile(outPath);

            process.StartProfiling(outPath);
            _profilingProcess = process;
            _stopCommand.Enabled = true;
            _startCommand.Enabled = false;
            _startCommandCtx.Enabled = false;
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowPeformanceExplorer(object sender, EventArgs e)
        {
            ShowPerformanceExplorer();
        }

        internal PerfToolWindow ShowPerformanceExplorer()
        {
            var pane = this.FindToolWindow(typeof(PerfToolWindow), 0, true);
            if (pane == null)
            {
                throw new InvalidOperationException();
            }
            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            if (frame == null)
            {
                throw new InvalidOperationException();
            }

            ErrorHandler.ThrowOnFailure(frame.Show());
            return pane as PerfToolWindow;
        }

        private void AddPerformanceSession(object sender, EventArgs e)
        {
            string baseName = "Performance";
            var target = new ProfilingTarget();
            AddPerformanceSession(baseName, target);
        }

        private SessionNode AddPerformanceSession(string baseName, ProfilingTarget target)
        {
            var dte = (EnvDTE.DTE)NodejsProfilingPackage.GetGlobalService(typeof(EnvDTE.DTE));

            string filename;
            int? id = null;
            bool save = false;
            do
            {
                filename = baseName + id + PerfFileType;
                if (dte.Solution.IsOpen && !String.IsNullOrEmpty(dte.Solution.FullName))
                {
                    filename = Path.Combine(Path.GetDirectoryName(dte.Solution.FullName), filename);
                    save = true;
                }
                id = (id ?? 0) + 1;
            } while (File.Exists(filename));
            return ShowPerformanceExplorer().Sessions.AddTarget(target, filename, save);
        }

        private void StartProfiling(object sender, EventArgs e)
        {
            ShowPerformanceExplorer().Sessions.StartProfiling();
        }

        private void StopProfiling(object sender, EventArgs e)
        {
            var process = _profilingProcess;
            if (process != null)
            {
                process.StopProfiling();
            }
        }

        private void IsProfilingActiveAndSessionsExist(object sender, EventArgs args)
        {
            var oleMenu = sender as OleMenuCommand;
            if (_profilingProcess != null)
            {
                oleMenu.Enabled = false;
            }
            else
            {
                if (PerfToolWindow.Instance != null && PerfToolWindow.Instance.Sessions.Sessions.Count > 0)
                {
                    oleMenu.Enabled = true;
                }
                else
                {
                    oleMenu.Enabled = false;
                }
            }
        }

        private void IsProfilingInactive(object sender, EventArgs args)
        {
            var oleMenu = sender as OleMenuCommand;

            if (_profilingProcess != null)
            {
                oleMenu.Enabled = true;
            }
            else
            {
                oleMenu.Enabled = false;
            }
        }

        private void IsProfilingActive(object sender, EventArgs args)
        {
            var oleMenu = sender as OleMenuCommand;

            if (_profilingProcess != null)
            {
                oleMenu.Enabled = false;
            }
            else
            {
                oleMenu.Enabled = true;
            }
        }

        public bool IsProfiling
        {
            get
            {
                return _profilingProcess != null;
            }
        }

        internal Guid GetStartupProjectGuid()
        {
            var buildMgr = (IVsSolutionBuildManager)GetService(typeof(IVsSolutionBuildManager));
            IVsHierarchy hierarchy;
            if (buildMgr != null && ErrorHandler.Succeeded(buildMgr.get_StartupProject(out hierarchy)) && hierarchy != null)
            {
                Guid guid;
                if (ErrorHandler.Succeeded(hierarchy.GetGuidProperty(
                    (uint)VSConstants.VSITEMID.Root,
                    (int)__VSHPROPID.VSHPROPID_ProjectIDGuid,
                    out guid
                )))
                {
                    return guid;
                }
            }
            return Guid.Empty;
        }

        internal IVsSolution Solution
        {
            get
            {
                return GetService(typeof(SVsSolution)) as IVsSolution;
            }
        }
    }
}

