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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using EnvDTE;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace Microsoft.NodejsTools.Project {
    internal class NodeModulesNode : AbstractNpmNode {
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

        private readonly GlobalModulesNode _globalModulesNode;
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
            : base(root) {
            CreateNpmController();

            _globalModulesNode = new GlobalModulesNode(root, this);
            AddChild(_globalModulesNode);

            _devModulesNode = new LocalModulesNode(root, this, "dev", "DevelopmentModules");
            AddChild(_devModulesNode);

            _optionalModulesNode = new LocalModulesNode(root, this, "optional", "OptionalModules");
            AddChild(_optionalModulesNode);
        }

        private void CheckNotDisposed() {
            if (_isDisposed) {
                throw new ObjectDisposedException(
                    "This NodeModulesNode has been disposed of and should no longer be used.");
            }
        }

        protected override void Dispose(bool disposing) {
            if (!_isDisposed) {
                _npmController.Dispose();

                if (null != _npmIdleTimer) {
                    _npmIdleTimer.Dispose();
                    _npmIdleTimer = null;
                }

                if (null != _npmController) {
                    _npmController.CommandStarted -= NpmController_CommandStarted;
                    _npmController.OutputLogged -= NpmController_OutputLogged;
                    _npmController.ErrorLogged -= NpmController_ErrorLogged;
                    _npmController.ExceptionLogged -= NpmController_ExceptionLogged;
                    _npmController.CommandCompleted -= NpmController_CommandCompleted;
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        private string GetNpmPathFromNodePathInProject() {
            var props = ProjectMgr.NodeProperties as NodejsProjectNodeProperties;
            if (null != props) {
                var nodePath = props.NodeExePath;
                if (!string.IsNullOrEmpty(nodePath)) {
                    var dir = Path.GetDirectoryName(nodePath);
                    return string.IsNullOrEmpty(dir) ? null : Path.Combine(dir, "npm.cmd");
                }
            }
            return null;
        }

        private class NpmPathProvider : INpmPathProvider {
            private NodeModulesNode _owner;
            internal NpmPathProvider(NodeModulesNode owner) {
                _owner = owner;
            }

            public string PathToNpm {
                get {
                    return _owner.GetNpmPathFromNodePathInProject();
                }
            }
        }

        private INpmController CreateNpmController() {
            if (null == _npmController) {
                _npmController = NpmControllerFactory.Create(
                    _projectNode.ProjectHome,
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Microsoft",
                        "Node.js Tools",
                        "packagecache.json"
                    ),
                    false,
                    new NpmPathProvider(this));
                _npmController.CommandStarted += NpmController_CommandStarted;
                _npmController.OutputLogged += NpmController_OutputLogged;
                _npmController.ErrorLogged += NpmController_ErrorLogged;
                _npmController.ExceptionLogged += NpmController_ExceptionLogged;
                _npmController.CommandCompleted += NpmController_CommandCompleted;
            }
            return _npmController;
        }

        void NpmController_FinishedRefresh(object sender, EventArgs e) {
            ReloadHierarchySafe();
        }

        public INpmController NpmController {
            get {
                return _npmController;
            }
        }

        internal IRootPackage RootPackage {
            get {
                var controller = NpmController;
                return null == controller ? null : controller.RootPackage;
            }
        }

        private INodeModules RootModules {
            get {
                var root = RootPackage;
                return null == root ? null : root.Modules;
            }
        }

        private bool HasMissingModules {
            get {
                var modules = RootModules;
                return null != modules && modules.HasMissingModules;
            }
        }

        private bool HasModules {
            get {
                var modules = RootModules;
                return null != modules && modules.Count > 0;
            }
        }

        #endregion

        #region Logging and status bar updates

        private static readonly Guid NpmOutputPaneGuid = new Guid("25764421-33B8-4163-BD02-A94E299D52D8");

        private OutputWindowRedirector GetNpmOutputPane() {
            try {
                return OutputWindowRedirector.Get(_projectNode.Site, NpmOutputPaneGuid, SR.GetString(SR.NpmOutputPaneTitle));
            } catch (InvalidOperationException) {
                return null;
            }
        }

        private void ConditionallyShowNpmOutputPane() {
            if (NodejsPackage.Instance.GeneralOptionsPage.ShowOutputWindowWhenExecutingNpm) {
                var pane = GetNpmOutputPane();
                if (null != pane) {
                    pane.ShowAndActivate();
                }
            }
        }

#if INTEGRATE_WITH_ERROR_LIST

        private ErrorListProvider _errorListProvider;

        private ErrorListProvider GetErrorListProvider() {
            if (null == _errorListProvider) {
                _errorListProvider = new ErrorListProvider(_projectNode.ProjectMgr.Site);
            }
            return _errorListProvider;
        }

        private void WriteNpmErrorsToErrorList(NpmLogEventArgs args) {
            var provider = GetErrorListProvider();
            foreach (var line in args.LogText.Split(new[] {'\n' })) {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("npm ERR!")) {
                    provider.Tasks.Add(new ErrorTask() {
                        Category = TaskCategory.User,
                        ErrorCategory = TaskErrorCategory.Error,
                        Text = trimmed
                    });
                } else if (trimmed.StartsWith("npm WARN")) {
                    provider.Tasks.Add(new ErrorTask() {
                        Category = TaskCategory.User,
                        ErrorCategory = TaskErrorCategory.Warning,
                        Text = trimmed
                    });
                }
            }
        }

#endif

        private void ForceUpdateStatusBarWithNpmActivity(string activity) {
            if (string.IsNullOrEmpty(activity) || string.IsNullOrEmpty(activity.Trim())) {
                return;
            }

            if (!activity.Contains("npm")) {
                activity = string.Format("npm: {0}", activity);
            }

            var statusBar = (IVsStatusbar)_projectNode.GetService(typeof(SVsStatusbar));
            if (null != statusBar) {
                statusBar.SetText(activity);
            }
        }

        private void ForceUpdateStatusBarWithNpmActivitySafe(string activity) {
            UIThread.InvokeAsync(() => ForceUpdateStatusBarWithNpmActivity(activity))
                .HandleAllExceptions(SR.ProductName)
                .DoNotWait();
        }

        private void UpdateStatusBarWithNpmActivity(string activity) {
            lock (_commandCountLock) {
                if (_npmCommandsExecuting == 0) {
                    return;
                }
            }

            ForceUpdateStatusBarWithNpmActivitySafe(activity);
        }

        private static string TrimLastNewline(string text) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            if (text.EndsWith("\r\n")) {
                return text.Remove(text.Length - 2);
            }
            if (text.EndsWith("\r") || text.EndsWith("\n")) {
                return text.Remove(text.Length - 1);
            }

            return text;
        }

        private void WriteNpmLogToOutputWindow(string logText) {
            var pane = GetNpmOutputPane();
            if (null != pane) {
                pane.WriteLine(logText);
            }

            UpdateStatusBarWithNpmActivity(logText);

#if INTEGRATE_WITH_ERROR_LIST
            WriteNpmErrorsToErrorList(args);
#endif
        }

        private void WriteNpmLogToOutputWindow(NpmLogEventArgs args) {
            WriteNpmLogToOutputWindow(TrimLastNewline(args.LogText));
        }

        private void NpmController_CommandStarted(object sender, EventArgs e) {
            StopNpmIdleTimer();
            lock (_commandCountLock) {
                ++_npmCommandsExecuting;
            }
        }

        private void NpmController_ErrorLogged(object sender, NpmLogEventArgs e) {
            WriteNpmLogToOutputWindow(e);
        }

        private void NpmController_OutputLogged(object sender, NpmLogEventArgs e) {
            WriteNpmLogToOutputWindow(e);
        }

        private void NpmController_ExceptionLogged(object sender, NpmExceptionEventArgs e) {
            WriteNpmLogToOutputWindow(ErrorHelper.GetExceptionDetailsText(e.Exception));
        }

        private void NpmController_CommandCompleted(object sender, NpmCommandCompletedEventArgs e) {
            lock (_commandCountLock) {
                --_npmCommandsExecuting;
                if (_npmCommandsExecuting < 0) {
                    _npmCommandsExecuting = 0;
                }
            }

            string message;
            if (e.WithErrors) {
                message = SR.GetString(
                    e.Cancelled ? SR.NpmCancelledWithErrors : SR.NpmCompletedWithErrors,
                    e.CommandText
                );
            } else if (e.Cancelled) {
                message = SR.GetString(SR.NpmCancelled, e.CommandText);
            } else {
                message = SR.GetString(SR.NpmSuccessfullyCompleted, e.CommandText);
            }

            ForceUpdateStatusBarWithNpmActivitySafe(message);

            RestartNpmIdleTimer();
        }

        private void StopNpmIdleTimer() {
            if (null != _npmIdleTimer) {
                _npmIdleTimer.Dispose();
            }
        }

        private void RestartNpmIdleTimer() {
            StopNpmIdleTimer();
            _npmIdleTimer = new Timer(CheckForLongPaths, null, 1000, Timeout.Infinite);
        }

        private void CheckForLongPaths(object state) {
            UIThread.Invoke(() => _projectNode.CheckForLongPaths().HandleAllExceptions(SR.ProductName).DoNotWait());
        }

        #endregion

        #region Updating module hierarchy

        internal void ReloadHierarchySafe() {
            UIThread.InvokeAsync(ReloadHierarchy)
                .HandleAllExceptions(SR.ProductName)
                .DoNotWait();
        }

        private void ReloadHierarchy() {
            if (ProjectMgr.IsClosed) {
                return;
            }

            var controller = _npmController;
            if (null != controller) {
                if (null != RootPackage) {
                    var dev = controller.RootPackage.Modules.Where(package => package.IsDevDependency);
                    _devModulesNode.Packages = dev;
                    ReloadHierarchy(_devModulesNode, dev);

                    var optional = controller.RootPackage.Modules.Where(package => package.IsOptionalDependency);
                    _optionalModulesNode.Packages = optional;
                    ReloadHierarchy(_optionalModulesNode, optional);

                    var root = controller.RootPackage.Modules.Where(package => 
                        package.IsDependency || 
                        !package.IsListedInParentPackageJson);
                    
                    ReloadHierarchy(this, root);
                }

                var global = controller.GlobalPackages;
                if (null != global) {
                    _globalModulesNode.GlobalPackages = global;
                    ReloadHierarchy(_globalModulesNode, global.Modules);
                }

                if (_firstHierarchyLoad) {
                    controller.FinishedRefresh += NpmController_FinishedRefresh;
                    _firstHierarchyLoad = false;
                }
            }
        }

        #endregion

        #region HierarchyNode implementation

        public override int SortPriority {
            get { return DefaultSortOrderNode.ReferenceContainerNode + 1; }
        }

        public override string Url {
            get { return NodeModulesVirtualName; }
        }

        public override string Caption {
            get { return _cCaption; }
        }

        #endregion

        #region Command handling

        internal bool IsCurrentStateASuppressCommandsMode() {
            return _suppressCommands || ProjectMgr.IsCurrentStateASuppressCommandsMode();
        }

        private void SuppressCommands() {
            _suppressCommands = true;
        }

        private void AllowCommands() {
            _suppressCommands = false;
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmManageModules:
                        result = IsCurrentStateASuppressCommandsMode()
                            ? QueryStatusResult.SUPPORTED
                            : QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmInstallModules:
                        if (IsCurrentStateASuppressCommandsMode()) {
                            result = QueryStatusResult.SUPPORTED;
                        } else {
                            if (HasMissingModules) {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            } else {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateModules:
                        if (IsCurrentStateASuppressCommandsMode()) {
                            result = QueryStatusResult.SUPPORTED;
                        } else {
                            if (HasModules) {
                                result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                            } else {
                                result = QueryStatusResult.SUPPORTED;
                            }
                        }
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmInstallSingleMissingModule:
                    case PkgCmdId.cmdidNpmUninstallModule:
                    case PkgCmdId.cmdidNpmUpdateSingleModule:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmManageModules:
                        ManageModules();
                        return VSConstants.S_OK;

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

        public void ManageModules() {
            CheckNotDisposed();

            if (NpmController.RootPackage == null) {
                NpmController.Refresh();
                if (NpmController.RootPackage == null) {
                    MessageBox.Show(String.Format("Unable to parse {0} from your project.  Please fix any errors and try again.", NodejsConstants.PackageJsonFile));
                    return;
                }
            }

            using (var executeVm = new NpmOutputViewModel(NpmController))
            using (var manager = new NpmPackageInstallWindow(NpmController, executeVm)) {
                manager.Owner = System.Windows.Application.Current.MainWindow;
                manager.ShowModal();
            }
            ReloadHierarchy();
        }

        private void DoPreCommandActions() {
            CheckNotDisposed();
            SuppressCommands();
            ConditionallyShowNpmOutputPane();
        }

        private bool CheckValidCommandTarget(DependencyNode node) {
            if (null == node) {
                return false;
            }
            var props = node.GetPropertiesObject();
            if (null == props || props.IsSubPackage) {
                return false;
            }
            var package = node.Package;
            if (null == package) {
                return false;
            }
            return true;
        }

        public async System.Threading.Tasks.Task InstallMissingModules() {
            DoPreCommandActions();
            try {
                using (var commander = NpmController.CreateNpmCommander()) {
                    await commander.Install();
                }
            } catch (NpmNotFoundException nnfe) {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally {
                AllowCommands();
            }
        }

        public async System.Threading.Tasks.Task InstallMissingModule(DependencyNode node) {
            if (!CheckValidCommandTarget(node)) {
                return;
            }

            var root = _npmController.RootPackage;
            if (null == root) {
                return;
            }

            var pkgJson = root.PackageJson;
            if (null == pkgJson) {
                return;
            }

            var package = node.Package;
            var dep = root.PackageJson.AllDependencies[package.Name];

            DoPreCommandActions();
            try {
                using (var commander = NpmController.CreateNpmCommander()) {
                    if (node.GetPropertiesObject().IsGlobalInstall) {
                        //  I genuinely can't see a way this would ever happen but, just to be on the safe side...
                        await commander.InstallGlobalPackageByVersionAsync(
                            package.Name,
                            null == dep ? "*" : dep.VersionRangeText);
                    } else {
                        await commander.InstallPackageByVersionAsync(
                            package.Name,
                            null == dep ? "*" : dep.VersionRangeText,
                            DependencyType.Standard,
                            false);
                    }
                }
            } catch (NpmNotFoundException nnfe) {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally {
                AllowCommands();
            }
        }

        internal async System.Threading.Tasks.Task UpdateModules(IList<HierarchyNode> nodes) {
            DoPreCommandActions();
            try {
                using (var commander = NpmController.CreateNpmCommander()) {
                    if (nodes.Count == 1 && nodes[0] == this) {
                        await commander.UpdatePackagesAsync();
                    } else {
                        var valid = nodes.OfType<DependencyNode>().Where(CheckValidCommandTarget).ToList();

                        var list = valid.Where(node => node.GetPropertiesObject().IsGlobalInstall).Select(node => node.Package).ToList();
                        if (list.Count > 0) {
                            await commander.UpdateGlobalPackagesAsync(list);
                        }

                        list = valid.Where(node => !node.GetPropertiesObject().IsGlobalInstall).Select(node => node.Package).ToList();
                        if (list.Count > 0) {
                            await commander.UpdatePackagesAsync(list);
                        }
                    }
                }
            } catch (NpmNotFoundException nnfe) {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally {
                AllowCommands();
            }
        }

        public void UpdateModules() {
            var t = UpdateModules(_projectNode.GetSelectedNodes());
        }

        public async System.Threading.Tasks.Task UpdateModule(DependencyNode node) {
            if (!CheckValidCommandTarget(node)) {
                return;
            }
            DoPreCommandActions();
            try {
                using (var commander = NpmController.CreateNpmCommander()) {
                    if (node.GetPropertiesObject().IsGlobalInstall) {
                        await commander.UpdateGlobalPackagesAsync(new[] { node.Package });
                    } else {
                        await commander.UpdatePackagesAsync(new[] { node.Package });
                    }
                }
            } catch (NpmNotFoundException nnfe) {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally {
                AllowCommands();
            }
        }

        public async System.Threading.Tasks.Task UninstallModules() {
            DoPreCommandActions();
            try {
                var selected = _projectNode.GetSelectedNodes();
                using (var commander = NpmController.CreateNpmCommander()) {
                    foreach (var node in selected.OfType<DependencyNode>().Where(CheckValidCommandTarget)) {
                        if (node.GetPropertiesObject().IsGlobalInstall) {
                            await commander.UninstallGlobalPackageAsync(node.Package.Name);
                        } else {
                            await commander.UninstallPackageAsync(node.Package.Name);
                        }
                    }
                }
            } catch (NpmNotFoundException nnfe) {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally {
                AllowCommands();
            }
        }

        public async System.Threading.Tasks.Task UninstallModule(DependencyNode node) {
            if (!CheckValidCommandTarget(node)) {
                return;
            }
            DoPreCommandActions();
            try {
                using (var commander = NpmController.CreateNpmCommander()) {
                    if (node.GetPropertiesObject().IsGlobalInstall) {
                        await commander.UninstallGlobalPackageAsync(node.Package.Name);
                    } else {
                        await commander.UninstallPackageAsync(node.Package.Name);
                    }
                }
            } catch (NpmNotFoundException nnfe) {
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally {
                AllowCommands();
            }
        }

        #endregion
    }
}