// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace Microsoft.NodejsTools.Project
{
    internal class NodeModulesNode : AbstractNpmNode
    {
        #region Constants

        /// <summary>
        /// The caption to display for this node
        /// </summary>
        private const string _cCaption = "npm";

        /// <summary>
        /// The virtual name of this node.
        /// </summary>
        public const string NodeModulesVirtualName = "NodeModules";

        #endregion

        #region Member variables

        private readonly LocalModulesNode _devModulesNode;
        private readonly LocalModulesNode _optionalModulesNode;

        private Timer _npmIdleTimer;
        private INpmController _npmController;
        private int _npmCommandsExecuting;
        private bool _suppressCommands;
        private bool _firstHierarchyLoad = true;

        private readonly object _commandCountLock = new object();

        private bool _isDisposed;

        #endregion

        #region Initialization

        public NodeModulesNode(NodejsProjectNode root)
            : base(root)
        {
            this._npmController = DefaultNpmController(this._projectNode.ProjectHome, new NpmPathProvider(this));
            RegisterWithNpmController(this._npmController);

            this._devModulesNode = new LocalModulesNode(root, this, "dev", "DevelopmentModules", DependencyType.Development);
            AddChild(this._devModulesNode);

            this._optionalModulesNode = new LocalModulesNode(root, this, "optional", "OptionalModules", DependencyType.Optional);
            AddChild(this._optionalModulesNode);
        }

        private void CheckNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(
                    "This NodeModulesNode has been disposed of and should no longer be used.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                this._npmController.Dispose();

                if (null != this._npmIdleTimer)
                {
                    this._npmIdleTimer.Dispose();
                    this._npmIdleTimer = null;
                }

                if (null != this._npmController)
                {
                    this._npmController.CommandStarted -= this.NpmController_CommandStarted;
                    this._npmController.OutputLogged -= this.NpmController_OutputLogged;
                    this._npmController.ErrorLogged -= this.NpmController_ErrorLogged;
                    this._npmController.ExceptionLogged -= this.NpmController_ExceptionLogged;
                    this._npmController.CommandCompleted -= this.NpmController_CommandCompleted;
                }

                this._devModulesNode.Dispose();
                this._optionalModulesNode.Dispose();

                this._isDisposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        private string GetNpmPathFromNodePathInProject()
        {
            var props = this.ProjectMgr.NodeProperties as NodejsProjectNodeProperties;
            return NpmHelpers.GetPathToNpm(props != null ? props.NodeExePath : null);
        }

        private class NpmPathProvider : INpmPathProvider
        {
            private NodeModulesNode _owner;
            internal NpmPathProvider(NodeModulesNode owner)
            {
                this._owner = owner;
            }

            public string PathToNpm => this._owner.GetNpmPathFromNodePathInProject();
        }

        private static INpmController DefaultNpmController(string projectHome, NpmPathProvider pathProvider)
        {
            return NpmControllerFactory.Create(
                projectHome,
                NodejsConstants.NpmCachePath,
                false,
                pathProvider);
        }

        private void RegisterWithNpmController(INpmController controller)
        {
            controller.CommandStarted += this.NpmController_CommandStarted;
            controller.OutputLogged += this.NpmController_OutputLogged;
            controller.ErrorLogged += this.NpmController_ErrorLogged;
            controller.ExceptionLogged += this.NpmController_ExceptionLogged;
            controller.CommandCompleted += this.NpmController_CommandCompleted;
        }

        private void NpmController_FinishedRefresh(object sender, EventArgs e)
        {
            ReloadHierarchySafe();
        }

        public INpmController NpmController => this._npmController;

        internal IRootPackage RootPackage => this.NpmController?.RootPackage;

        private INodeModules RootModules => this.RootPackage?.Modules;

        private bool HasMissingModules
        {
            get
            {
                var modules = this.RootModules;
                return null != modules && modules.HasMissingModules;
            }
        }

        private bool HasModules
        {
            get
            {
                var modules = this.RootModules;
                return null != modules && modules.Count > 0;
            }
        }

        #endregion

        #region Logging and status bar updates

        private OutputWindowRedirector NpmOutputPane => this._projectNode.NpmOutputPane;

        private void ForceUpdateStatusBarWithNpmActivity(string activity)
        {
            if (string.IsNullOrEmpty(activity) || string.IsNullOrEmpty(activity.Trim()))
            {
                return;
            }

            if (!activity.Contains("npm"))
            {
                activity = string.Format(CultureInfo.CurrentCulture, "npm: {0}", activity);
            }

            var statusBar = (IVsStatusbar)this._projectNode.GetService(typeof(SVsStatusbar));
            if (null != statusBar)
            {
                statusBar.SetText(activity);
            }
        }

        private void ForceUpdateStatusBarWithNpmActivitySafe(string activity)
        {
            this.ProjectMgr.Site.GetUIThread().InvokeAsync(() => ForceUpdateStatusBarWithNpmActivity(activity))
                .HandleAllExceptions(SR.ProductName)
                .DoNotWait();
        }

        private void UpdateStatusBarWithNpmActivity(string activity)
        {
            lock (this._commandCountLock)
            {
                if (this._npmCommandsExecuting == 0)
                {
                    return;
                }
            }

            ForceUpdateStatusBarWithNpmActivitySafe(activity);
        }

        private static string TrimLastNewline(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (text.EndsWith("\r\n", StringComparison.Ordinal))
            {
                return text.Remove(text.Length - 2);
            }
            if (text.EndsWith("\r", StringComparison.Ordinal) || text.EndsWith("\n", StringComparison.Ordinal))
            {
                return text.Remove(text.Length - 1);
            }

            return text;
        }

        private void WriteNpmLogToOutputWindow(string logText)
        {
            this.NpmOutputPane?.WriteLine(logText);
        }

        private void WriteNpmLogToOutputWindow(NpmLogEventArgs args)
        {
            WriteNpmLogToOutputWindow(TrimLastNewline(args.LogText));
        }

        private void NpmController_CommandStarted(object sender, EventArgs e)
        {
            StopNpmIdleTimer();
            lock (this._commandCountLock)
            {
                ++this._npmCommandsExecuting;
            }
        }

        private void NpmController_ErrorLogged(object sender, NpmLogEventArgs e)
        {
            WriteNpmLogToOutputWindow(e);
        }

        private void NpmController_OutputLogged(object sender, NpmLogEventArgs e)
        {
            WriteNpmLogToOutputWindow(e);
        }

        private void NpmController_ExceptionLogged(object sender, NpmExceptionEventArgs e)
        {
            WriteNpmLogToOutputWindow(ErrorHelper.GetExceptionDetailsText(e.Exception));
        }

        private void NpmController_CommandCompleted(object sender, NpmCommandCompletedEventArgs e)
        {
            lock (this._commandCountLock)
            {
                --this._npmCommandsExecuting;
                if (this._npmCommandsExecuting < 0)
                {
                    this._npmCommandsExecuting = 0;
                }
            }

            var message = GetStatusBarMessage(e);
            ForceUpdateStatusBarWithNpmActivitySafe(message);

            StopNpmIdleTimer();
            this._npmIdleTimer = new Timer(
                _ => this.ProjectMgr.Site.GetUIThread().Invoke(() => this._projectNode.CheckForLongPaths(e.Arguments).HandleAllExceptions(SR.ProductName).DoNotWait()),
                null, 1000, Timeout.Infinite);
        }

        private static string GetStatusBarMessage(NpmCommandCompletedEventArgs e)
        {
            if (e.WithErrors)
            {
                return string.Format(CultureInfo.CurrentCulture,
                    e.Cancelled ? Resources.NpmCancelledWithErrors : Resources.NpmCompletedWithErrors,
                    e.CommandText);
            }
            else if (e.Cancelled)
            {
                return string.Format(CultureInfo.CurrentCulture, Resources.NpmCancelled, e.CommandText);
            }
            return string.Format(CultureInfo.CurrentCulture, Resources.NpmSuccessfullyCompleted, e.CommandText);
        }

        private void StopNpmIdleTimer()
        {
            if (null != this._npmIdleTimer)
            {
                this._npmIdleTimer.Dispose();
            }
        }

        #endregion

        #region Updating module hierarchy

        internal void ReloadHierarchySafe()
        {
            NodejsPackage.Instance.GetUIThread().InvokeAsync(this.ReloadHierarchy)
                .HandleAllExceptions(SR.ProductName)
                .DoNotWait();
        }

        private void ReloadHierarchy()
        {
            if (this.ProjectMgr.IsClosed)
            {
                return;
            }

            var controller = this._npmController;
            if (null == controller)
            {
                return;
            }

            ReloadPackageHierarchies(controller);

            if (this._firstHierarchyLoad)
            {
                controller.FinishedRefresh += this.NpmController_FinishedRefresh;
                this._firstHierarchyLoad = false;
            }
        }

        private void ReloadPackageHierarchies(INpmController controller)
        {
            ReloadDevPackageHierarchy(controller);
            ReloadOptionalPackageHierarchy(controller);
            ReloadRootPackageHierarchy(controller);
        }

        private void ReloadRootPackageHierarchy(INpmController controller)
        {
            var root = GetRootPackages(controller);
            ReloadHierarchy(this, root);
        }

        private void ReloadOptionalPackageHierarchy(INpmController controller)
        {
            var optional = GetOptionalPackages(controller);
            this._optionalModulesNode.Packages = optional;
            ReloadHierarchy(this._optionalModulesNode, optional);
        }

        private void ReloadDevPackageHierarchy(INpmController controller)
        {
            var dev = GetDevPackages(controller);
            this._devModulesNode.Packages = dev;
            ReloadHierarchy(this._devModulesNode, dev);
        }

        #endregion

        #region HierarchyNode implementation

        public override int SortPriority => DefaultSortOrderNode.ReferenceContainerNode + 1;
        public override string Url => NodeModulesVirtualName;
        public override string Caption => _cCaption;
        #endregion

        #region Command handling

        internal bool IsCurrentStateASuppressCommandsMode()
        {
            return this._suppressCommands || this.ProjectMgr.IsCurrentStateASuppressCommandsMode();
        }

        private void SuppressCommands()
        {
            this._suppressCommands = true;
        }

        private void AllowCommands()
        {
            this._suppressCommands = false;
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == Guids.NodejsNpmCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmInstallModules:
                        if (IsCurrentStateASuppressCommandsMode())
                        {
                            result = QueryStatusResult.SUPPORTED;
                        }
                        else
                        {
                            if (this.HasMissingModules)
                            {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            }
                            else
                            {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateModules:
                        if (IsCurrentStateASuppressCommandsMode())
                        {
                            result = QueryStatusResult.SUPPORTED;
                        }
                        else
                        {
                            if (this.HasModules)
                            {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            }
                            else
                            {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                    case PkgCmdId.cmdidNpmUninstallModule:
                    case PkgCmdId.cmdidNpmUpdateSingleModule:
                    case PkgCmdId.cmdidNpmOpenModuleHomepage:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Guids.NodejsNpmCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmInstallModules:
                        var t = InstallMissingModules();
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateModules:
                        UpdateModules();
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        public void ManageModules(DependencyType dependencyType = DependencyType.Standard)
        {
            CheckNotDisposed();

            if (this.NpmController.RootPackage == null)
            {
                this.NpmController.Refresh();
                if (this.NpmController.RootPackage == null)
                {
                    MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.NodeModulesCouldNotParsePackageJsonErrorMessageText, NodejsConstants.PackageJsonFile));
                    return;
                }
            }

            using (var npmWorker = new NpmWorker(this.NpmController))
            using (var manager = new NpmPackageInstallWindow(this.NpmController, npmWorker, dependencyType))
            {
                manager.Owner = System.Windows.Application.Current.MainWindow;
                manager.ShowModal();
            }
            ReloadHierarchy();
        }

        private void DoPreCommandActions()
        {
            CheckNotDisposed();
            SuppressCommands();
        }

        private bool CheckValidCommandTarget(DependencyNode node)
        {
            if (null == node)
            {
                return false;
            }
            var props = node.GetPropertiesObject();
            if (null == props || props.IsSubPackage)
            {
                return false;
            }
            var package = node.Package;
            if (null == package)
            {
                return false;
            }
            return true;
        }

        private async System.Threading.Tasks.Task RunNpmCommand(Func<INpmCommander, System.Threading.Tasks.Task> impl)
        {
            DoPreCommandActions();
            try
            {
                using (var commander = this.NpmController.CreateNpmCommander())
                {
                    await impl(commander);
                }
            }
            catch (NpmNotFoundException nnfe)
            {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            }
            finally
            {
                AllowCommands();
            }
        }

        public System.Threading.Tasks.Task InstallMissingModules()
        {
            return RunNpmCommand(commander => commander.Install());
        }

        public async System.Threading.Tasks.Task InstallMissingModule(DependencyNode node)
        {
            if (!CheckValidCommandTarget(node))
            {
                return;
            }

            var root = this._npmController.RootPackage;
            if (null == root)
            {
                return;
            }

            var pkgJson = root.PackageJson;
            if (null == pkgJson)
            {
                return;
            }

            var package = node.Package;
            var dep = root.PackageJson.AllDependencies[package.Name];

            await RunNpmCommand(async commander =>
            {
                await commander.InstallPackageByVersionAsync(
                    package.Name,
                    null == dep ? "*" : dep.VersionRangeText,
                    DependencyType.Standard,
                    false);
            });
        }

        internal System.Threading.Tasks.Task UpdateModules(IList<HierarchyNode> nodes)
        {
            return RunNpmCommand(async commander =>
            {
                if (nodes.Count == 1 && nodes[0] == this)
                {
                    await commander.UpdatePackagesAsync();
                }
                else
                {
                    var valid = nodes.OfType<DependencyNode>().Where(this.CheckValidCommandTarget).ToList();
                    var list = valid.Select(node => node.Package).ToList();
                    if (list.Count > 0)
                    {
                        await commander.UpdatePackagesAsync(list);
                    }
                }
            });
        }

        public void UpdateModules()
        {
            var t = UpdateModules(this._projectNode.GetSelectedNodes());
        }

        public async System.Threading.Tasks.Task UpdateModule(DependencyNode node)
        {
            if (!CheckValidCommandTarget(node))
            {
                return;
            }
            await RunNpmCommand(async commander =>
            {
                await commander.UpdatePackagesAsync(new[] { node.Package });
            });
        }

        public System.Threading.Tasks.Task UninstallModules()
        {
            var selected = this._projectNode.GetSelectedNodes();
            return RunNpmCommand(async commander =>
            {
                foreach (var node in selected.OfType<DependencyNode>().Where(this.CheckValidCommandTarget))
                {
                    TelemetryHelper.LogUnInstallNpmPackage();
                    await commander.UninstallPackageAsync(node.Package.Name);
                }
            });
        }

        public async System.Threading.Tasks.Task UninstallModule(DependencyNode node)
        {
            if (!CheckValidCommandTarget(node))
            {
                return;
            }

            TelemetryHelper.LogUnInstallNpmPackage();

            await RunNpmCommand(async commander =>
            {
                await commander.UninstallPackageAsync(node.Package.Name);
            });
        }

        #endregion

        public override void ManageNpmModules()
        {
            ManageModules();
        }

        private static IEnumerable<IPackage> GetDevPackages(INpmController controller)
        {
            if (controller == null || controller.RootPackage == null)
                return Enumerable.Empty<IPackage>();
            return controller.RootPackage.Modules.Where(package => package.IsDevDependency);
        }

        private static IEnumerable<IPackage> GetOptionalPackages(INpmController controller)
        {
            if (controller == null || controller.RootPackage == null)
                return Enumerable.Empty<IPackage>();
            return controller.RootPackage.Modules.Where(package => package.IsOptionalDependency);
        }

        private static IEnumerable<IPackage> GetRootPackages(INpmController controller)
        {
            if (controller == null || controller.RootPackage == null)
                return Enumerable.Empty<IPackage>();
            return controller.RootPackage.Modules.Where(package =>
                package.IsDependency || !package.IsListedInParentPackageJson);
        }
    }
}

