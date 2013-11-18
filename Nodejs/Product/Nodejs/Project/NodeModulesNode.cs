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
        private const string _cCaption = "Node Modules";

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

            foreach (var command in NodejsPackage.Instance.NpmCommands){
                command.ModulesNode = this;
            }

            _watcher = new FileSystemWatcher(_projectNode.ProjectHome){
                NotifyFilter = NotifyFilters.LastWrite
            };
            _watcher.Changed += m_Watcher_Changed;
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
                    _watcher.Changed -= m_Watcher_Changed;
                    _watcher.Dispose();

                    if (null != _fileSystemWatcherTimer){
                        _fileSystemWatcherTimer.Dispose();
                        _fileSystemWatcherTimer = null;
                    }

                    if (null != _npmController){
                        _npmController.OutputLogged -= _npmController_OutputLogged;
                        _npmController.ErrorLogged -= _npmController_ErrorLogged;
                    }

                    foreach (var command in NodejsPackage.Instance.NpmCommands){
                        command.ModulesNode = null;
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

        private void m_Watcher_Changed(object sender, FileSystemEventArgs e){
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

        private void ReloadModules(){
            try{
                lock (_lock){
                    NpmController.Refresh();
                }
            } catch (PackageJsonException pje){
                MessageBox.Show(pje.Message, "Error Reading package.json", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (AggregateException ae){
                ErrorHelper.ReportNpmNotInstalled(null, ae);
            } catch (FileLoadException){    //  Fixes bug reported in work item 447 - just wait a bit and retry!
                RestartFileSystemWatcherTimer();
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

        private void WriteNpmLogToOutputWindow(string logText){
            var pane = GetNpmOutputPane();
            if (null != pane){
                pane.OutputStringThreadSafe(logText);
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
                if (!recycle.ContainsKey(package.Name)){
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
            get { return (int) PkgCmdId.menuIdNPM; }
        }

        #endregion

        #region Command handling

        public void BeforeQueryStatus(object source, EventArgs args){
            var command = source as OleMenuCommand;
            if (null == command){
                return;
            }

            switch (command.CommandID.ID){
                case PkgCmdId.cmdidNpmManageModules:
                    command.Enabled = true;
                    command.Visible = true;
                    break;

                case PkgCmdId.cmdidNpmUpdateModules:
                    command.Enabled = true;
                    command.Visible = true;
                    break;

                case PkgCmdId.cmdidNpmUninstallModule:
                    var selected = _projectNode.GetSelectedNodes();
                    bool enable = true;
                    foreach (var node in selected){
                        var dep = node as DependencyNode;
                        if (null == node || node.Parent != this) //  Don't want to let people uninstall sub-modules
                        {
                            enable = false;
                            break;
                        }
                    }
                    command.Enabled = enable;
                    command.Visible = enable;
                    break;
            }
        }

        public void ManageModules(){
            CheckNotDisposed();

            using (var manager = new PackageManagerDialog(NpmController)){
                manager.ShowDialog();
            }

            ReloadHierarchy();
        }

        public void UpdateModules(){
            CheckNotDisposed();

            try{
                var selected = _projectNode.GetSelectedNodes();
                using (var commander = NpmController.CreateNpmCommander()){
                    if (selected.Count == 1 && selected[0] == this){
                        commander.UpdatePackagesAsync();
                    } else{
                        commander.UpdatePackagesAsync(
                            selected.OfType<DependencyNode>().Select(dep => dep.Package).ToList());
                    }
                }
            } catch (NpmNotFoundException nnfe){
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            }
        }

        public void UninstallModules(){
            CheckNotDisposed();

            try{
                var selected = _projectNode.GetSelectedNodes();
                using (var commander = NpmController.CreateNpmCommander()){
                    foreach (var name in selected.OfType<DependencyNode>().Select(dep => dep.Package.Name).ToList()){
                        commander.UninstallPackageAsync(name);
                    }
                }
            } catch (NpmNotFoundException nnfe){
                ErrorHelper.ReportNpmNotInstalled(null, nnfe);
            }
        }

        #endregion
    }
}