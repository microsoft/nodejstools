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
using System.IO;
using System.Linq;
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
using Microsoft.VisualStudioTools.Project;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace Microsoft.NodejsTools.Project{
    internal class NodeModulesNode : HierarchyNode{
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

        private readonly NodejsProjectNode _projectNode;
        private readonly FileSystemWatcher _watcher;
        private Timer _fileSystemWatcherTimer;
        private INpmController _npmController; //  TODO: This is totally not the right place for this!!
        private readonly object _lock = new object();

        private bool _isDisposed;

        #endregion

        #region Initialisation

        public NodeModulesNode(NodejsProjectNode root) : base(root){
            _projectNode = root;
            ExcludeNodeFromScc = true;

            _watcher = new FileSystemWatcher(_projectNode.ProjectHome){
                NotifyFilter = NotifyFilters.LastWrite,
                IncludeSubdirectories = true
            };
            _watcher.Changed += Watcher_Modified;
            _watcher.Created += Watcher_Modified;
            _watcher.Deleted += Watcher_Modified;
            _watcher.EnableRaisingEvents = true;

            CreateNpmController();
        }

        private void CheckNotDisposed(){
            if (_isDisposed){
                throw new ObjectDisposedException(
                    "This NodeModulesNode has been disposed of and should no longer be used.");
            }
        }

        protected override void Dispose(bool disposing){
            if (! _isDisposed){
                lock (_lock){
                    _watcher.Changed -= Watcher_Modified;
                    _watcher.Created -= Watcher_Modified;
                    _watcher.Deleted -= Watcher_Modified;
                    _watcher.Dispose();

                    if (null != _fileSystemWatcherTimer){
                        _fileSystemWatcherTimer.Dispose();
                        _fileSystemWatcherTimer = null;
                    }

                    if (null != _npmController){
                        _npmController.OutputLogged -= _npmController_OutputLogged;
                        _npmController.ErrorLogged -= _npmController_ErrorLogged;
                    }
                }
                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        private string GetNpmPathFromNodePathInProject(){
            var props = ProjectMgr.NodeProperties as NodejsProjectNodeProperties;
            if (null != props){
                var nodePath = props.NodeExePath;
                if (!string.IsNullOrEmpty(nodePath)){
                    var dir = Path.GetDirectoryName(nodePath);
                    return string.IsNullOrEmpty(dir) ? null : Path.Combine(dir, "npm.cmd");
                }
            }
            return null;
        }

        private class NpmPathProvider : INpmPathProvider{
            private NodeModulesNode _owner;
            internal NpmPathProvider(NodeModulesNode owner){
                _owner = owner;
            }

            public string PathToNpm{
                get{
                    return _owner.GetNpmPathFromNodePathInProject();
                }
            }
        }

        private INpmController CreateNpmController(){
            lock (_lock){
                if (null == _npmController){
                    _npmController = NpmControllerFactory.Create(
                        _projectNode.BuildProject.DirectoryPath,
                        false,
                        new NpmPathProvider(this));
                    _npmController.OutputLogged += _npmController_OutputLogged;
                    _npmController.ErrorLogged += _npmController_ErrorLogged;
                    _npmController.ExceptionLogged += _npmController_ExceptionLogged;
                    ReloadModules();
                }
                return _npmController;
            }
        }

        public INpmController NpmController{
            get{
                return _npmController;
            }
        }

        #endregion

        #region Updating module hierarchy

        private void RestartFileSystemWatcherTimer(){
            lock (_lock)
            {
                if (null != _fileSystemWatcherTimer)
                {
                    _fileSystemWatcherTimer.Dispose();
                }

                _fileSystemWatcherTimer = new Timer(o => UpdateModulesFromTimer(), null, 1000, Timeout.Infinite);
            }
        }

        private void Watcher_Modified(object sender, FileSystemEventArgs e){
            string path = e.FullPath;
            if (!path.EndsWith("package.json") && !path.Contains("\\node_modules")){
                return;
            }

            RestartFileSystemWatcherTimer();
        }

        internal void ReloadHierarchySafe(){
            if (UIThread.Instance.IsUIThread)
            {
                ReloadHierarchy();
            }
            else
            {
                UIThread.Instance.Run(ReloadHierarchy);
            }
        }

        private void UpdateModulesFromTimer(){
            lock (_lock){
                if (null != _fileSystemWatcherTimer){
                    _fileSystemWatcherTimer.Dispose();
                    _fileSystemWatcherTimer = null;
                }

                ReloadModules();
            }

            ReloadHierarchySafe();
        }

        private int _refreshRetryCount;

        private void ReloadModules(){
            lock (_lock){
                var retry = false;
                Exception ex = null;
                try{
                    NpmController.Refresh();
                } catch (PackageJsonException pje){
                    retry = true;
                    ex = pje;
                } catch (AggregateException ae){
                    retry = true;
                    ex = ae;
                } catch (FileLoadException fle){
                    //  Fixes bug reported in work item 447 - just wait a bit and retry!
                    retry = true;
                    ex = fle;
                }

                if (retry){
                    if (_refreshRetryCount < 5){
                        ++_refreshRetryCount;
                        RestartFileSystemWatcherTimer();
                    } else {
                        WriteNpmLogToOutputWindow(ErrorHelper.GetExceptionDetailsText(ex));
                    }
                }
            }
        }

        private static readonly Guid NpmOutputPaneGuid = new Guid("25764421-33B8-4163-BD02-A94E299D52D8");

        private IVsOutputWindowPane GetNpmOutputPane(){
            var outputWindow = (IVsOutputWindow) _projectNode.GetService(typeof (SVsOutputWindow));
            IVsOutputWindowPane pane;
            if (outputWindow.GetPane(NpmOutputPaneGuid, out pane) != VSConstants.S_OK){
                outputWindow.CreatePane(NpmOutputPaneGuid, "Npm", 1, 1);
                outputWindow.GetPane(NpmOutputPaneGuid, out pane);
            }

            return pane;
        }

        private void ShowNpmOutputPane(){
            OutputWindowRedirector.GetGeneral(ProjectMgr.Package).ShowAndActivate();

            var pane = GetNpmOutputPane();
            pane.Activate();
        }

        private void ConditionallyShowNpmOutputPane(){
            if (NodejsPackage.Instance.GeneralOptionsPage.ShowOutputWindowWhenExecutingNpm){
                ShowNpmOutputPane();
            }
        }

#if INTEGRATE_WITH_ERROR_LIST

        private ErrorListProvider _errorListProvider;

        private ErrorListProvider GetErrorListProvider()
        {
            if (null == _errorListProvider)
            {
                _errorListProvider = new ErrorListProvider(_projectNode.ProjectMgr.Site);
            }
            return _errorListProvider;
        }

        private void WriteNpmErrorsToErrorList(NpmLogEventArgs args)
        {
            var provider = GetErrorListProvider();
            foreach (var line in args.LogText.Split(new[] {'\n' }))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("npm ERR!"))
                {
                    provider.Tasks.Add(new ErrorTask()
                    {
                        Category = TaskCategory.User,
                        ErrorCategory = TaskErrorCategory.Error,
                        Text = trimmed
                    });
                }
                else if (trimmed.StartsWith("npm WARN"))
                {
                    provider.Tasks.Add(new ErrorTask()
                    {
                        Category = TaskCategory.User,
                        ErrorCategory = TaskErrorCategory.Warning,
                        Text = trimmed
                    });
                }
            }
        }

        #endif

        private void UpdateStatusBarWithNpmActivity(string activity){
            if (string.IsNullOrEmpty(activity) || string.IsNullOrEmpty(activity.Trim())){
                return;
            }

            if (!activity.Contains("npm")){
                activity = string.Format("npm: {0}", activity);
            }

            var statusBar = (IVsStatusbar) _projectNode.GetService(typeof (SVsStatusbar));
            if (null != statusBar){
                statusBar.SetText(activity);
            }
        }

        private void WriteNpmLogToOutputWindow(string logText){
            var pane = GetNpmOutputPane();
            if (null != pane){
                pane.OutputStringThreadSafe(logText);
            }

            if (UIThread.Instance.IsUIThread)
            {
                UpdateStatusBarWithNpmActivity(logText);
            }
            else
            {
                UIThread.Instance.Run(() => UpdateStatusBarWithNpmActivity(logText));
            }

#if INTEGRATE_WITH_ERROR_LIST

            WriteNpmErrorsToErrorList(args);

#endif
        }

        private void WriteNpmLogToOutputWindow(NpmLogEventArgs args)
        {
            WriteNpmLogToOutputWindow(args.LogText);
        }

        private void _npmController_ErrorLogged(object sender, NpmLogEventArgs e){
            WriteNpmLogToOutputWindow(e);
        }

        private void _npmController_OutputLogged(object sender, NpmLogEventArgs e){
            WriteNpmLogToOutputWindow(e);
        }

        void _npmController_ExceptionLogged(object sender, NpmExceptionEventArgs e)
        {
            WriteNpmLogToOutputWindow(ErrorHelper.GetExceptionDetailsText(e.Exception));
        }

        private void ReloadHierarchy(){
            INpmController controller;

            lock (_lock){
                controller = _npmController;
            }

            if (null != controller){
                ReloadHierarchy(this, controller.RootPackage.Modules);
            }
        }

        private void ReloadHierarchy(HierarchyNode parent, INodeModules modules){
            //  We're going to reuse nodes for which matching modules exist in the new set.
            //  The reason for this is that we want to preserve the expansion state of the
            //  hierarchy. If we just bin everything off and recreate it all from scratch
            //  it'll all be in the collapsed state, which will be annoying for users who
            //  have drilled down into the hierarchy
            var recycle = new Dictionary<string, DependencyNode>();
            var remove = new List<HierarchyNode>();
            for (var current = parent.FirstChild; null != current; current = current.NextSibling){
                var dep = current as DependencyNode;
                if (null == dep){
                    remove.Add(current);
                    continue;
                }

                if (modules.Any(
                    module =>
                    module.Name == dep.Package.Name
                    && module.Version == dep.Package.Version
                    && module.IsBundledDependency == dep.Package.IsBundledDependency
                    && module.IsDevDependency == dep.Package.IsDevDependency
                    && module.IsListedInParentPackageJson == dep.Package.IsListedInParentPackageJson
                    && module.IsMissing == dep.Package.IsMissing
                    && module.IsOptionalDependency == dep.Package.IsOptionalDependency)){
                    recycle[dep.Package.Name] = dep;
                } else{
                    remove.Add(current);
                }
            }

            foreach (var obsolete in remove){
                parent.RemoveChild(obsolete);
                ProjectMgr.OnItemDeleted(obsolete);
            }

            foreach (var package in modules){
                DependencyNode child;

                if (recycle.ContainsKey(package.Name)){
                    child = recycle[package.Name];
                    child.Package = package;
                } else{
                    child = new DependencyNode(_projectNode, parent as DependencyNode, package);
                    parent.AddChild(child);
                }

                ReloadHierarchy(child, package.Modules);
                if (!recycle.ContainsKey(package.Name) && ProjectMgr.ParentHierarchy != null){
                    child.ExpandItem(EXPANDFLAGS.EXPF_CollapseFolder);
                }
            }
        }

        #endregion

        #region HierarchyNode implementation

        public override string GetEditLabel(){
            return null;
        }

        public override int SortPriority{
            get { return DefaultSortOrderNode.ReferenceContainerNode + 1; }
        }

        public override object GetIconHandle(bool open){
            return
                ProjectMgr.ImageHandler.GetIconHandle(
                    open ? (int) ProjectNode.ImageName.OpenReferenceFolder : (int) ProjectNode.ImageName.ReferenceFolder);
        }

        public override string Url{
            get { return NodeModulesVirtualName; }
        }

        public override string Caption{
            get { return _cCaption; }
        }

        public override Guid ItemTypeGuid{
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }

        public override int MenuCommandId{
            get { return VsMenus.IDM_VS_CTXT_ITEMNODE; }
        }

        #endregion

        #region Command handling

        private bool _suppressCommands;

        private bool IsCurrentStateASuppressCommandsMode(){
            return _suppressCommands || ProjectMgr.IsCurrentStateASuppressCommandsMode();
        }

        private void SuppressCommands(){
            _suppressCommands = true;
        }

        private void AllowCommands(){
            _suppressCommands = false;
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result){
            if (cmdGroup == GuidList.guidNodeCmdSet){
                switch (cmd){
                    case PkgCmdId.cmdidNpmManageModules:
                    case PkgCmdId.cmdidNpmInstallModules:
                    case PkgCmdId.cmdidNpmUpdateModules:
                    case PkgCmdId.cmdidNpmUninstallModule:
                        if (! IsCurrentStateASuppressCommandsMode()){
                            result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                        } else{
                            result = QueryStatusResult.SUPPORTED;
                        }
                        return VSConstants.S_OK;

                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut){
            if (cmdGroup == GuidList.guidNodeCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdId.cmdidNpmManageModules:
                        ManageModules();
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmInstallModules:
                        InstallMissingModules();
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUpdateModules:
                        UpdateModules();
                        return VSConstants.S_OK;

                    case PkgCmdId.cmdidNpmUninstallModule:
                        UninstallModules();
                        return VSConstants.S_OK;

                }
            }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        public void ManageModules(){
            CheckNotDisposed();

            using (var manager = new PackageManagerDialog(NpmController)){
                manager.ShowDialog();
            }

            ReloadHierarchy();
        }

        private void DoPreCommandActions(){
            CheckNotDisposed();
            SuppressCommands();
            ConditionallyShowNpmOutputPane();
        }

        public async void InstallMissingModules(){
            DoPreCommandActions();
            try{
                using (var commander = NpmController.CreateNpmCommander()){
                    await commander.Install();
                }
            } catch (NpmNotFoundException nnfe){
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally{
                AllowCommands();
            }
        }

        public async void UpdateModules(){
            DoPreCommandActions();
            try{
                var selected = _projectNode.GetSelectedNodes();
                using (var commander = NpmController.CreateNpmCommander()){
                    if (selected.Count == 1 && selected[0] == this){
                        await commander.UpdatePackagesAsync();
                    } else{
                        await commander.UpdatePackagesAsync(
                            selected.OfType<DependencyNode>().Select(dep => dep.Package).ToList());
                    }
                }
            } catch (NpmNotFoundException nnfe){
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally{
                AllowCommands();
            }
        }

        public async void UninstallModules(){
            DoPreCommandActions();
            try{
                var selected = _projectNode.GetSelectedNodes();
                using (var commander = NpmController.CreateNpmCommander()){
                    foreach (var name in selected.OfType<DependencyNode>().Select(dep => dep.Package.Name).ToList()){
                        await commander.UninstallPackageAsync(name);
                    }
                }
            } catch (NpmNotFoundException nnfe){
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            } finally{
                AllowCommands();
            }
        }

        #endregion
    }
}