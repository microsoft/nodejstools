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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;
using MSBuild = Microsoft.Build.Evaluation;


namespace Microsoft.NodejsTools.Project {
    class NodejsProjectNode : CommonProjectNode, VsWebSite.VSWebSite, Microsoft.NodejsTools.ProjectWizard.INodePackageModulesCommands {
        private VsProjectAnalyzer _analyzer;
        private readonly HashSet<string> _warningFiles = new HashSet<string>();
        private readonly HashSet<string> _errorFiles = new HashSet<string>();
        internal readonly RequireCompletionCache _requireCompletionCache = new RequireCompletionCache();
        private string _intermediateOutputPath;

        public NodejsProjectNode(NodejsProjectPackage package)
            : base(package, Utilities.GetImageList(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.Icons.NodejsImageList.bmp"))) {

            Type projectNodePropsType = typeof(NodejsProjectNodeProperties);
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
            InitDependencyImages();
        }

        public VsProjectAnalyzer Analyzer {
            get {
                return _analyzer;
            }
        }

        private static string[] _excludedAvailableItems = new[] { 
            "ApplicationDefinition", 
            "Page",
            "Resource",
            "SplashScreen",
            "DesignData",
            "DesignDataWithDesignTimeCreatableTypes",
            "EntityDeploy",
            "CodeAnalysisDictionary", 
            "XamlAppDef"
        };

        public override IEnumerable<string> GetAvailableItemNames() {
            // Remove a couple of available item names which show up from imports we
            // can't control out of Microsoft.Common.targets.
            return base.GetAvailableItemNames().Except(_excludedAvailableItems);
        }

        private void InitDependencyImages() {
            var images = ImageHandler.ImageList.Images;
            ImageIndexDependency = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.Dependency_16.png")));
            ImageIndexDependencyDev = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyDev_16.png")));
            ImageIndexDependnecyOptional = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyOptional_16.png")));
            ImageIndexDependencyNotListed = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyExtraneous_16.png")));
            ImageIndexDependencyBundled = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyBundled_16.png")));
            ImageIndexDependencyMissing = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyMissing_16.png")));
            ImageIndexDependencyDevMissing = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyDevMissing_16.png")));
            ImageIndexDependencyOptionalMissing = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyOptionalMissing_16.png")));
            ImageIndexDependencyBundledMissing = images.Count;
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.DependencyBundledMissing_16.png")));
        }

        public int ImageIndexDependency { get; private set; }
        public int ImageIndexDependencyDev { get; private set; }
        public int ImageIndexDependnecyOptional { get; private set; }
        public int ImageIndexDependencyNotListed { get; private set; }
        public int ImageIndexDependencyBundled { get; private set; }

        public int ImageIndexDependencyMissing { get; private set; }
        public int ImageIndexDependencyDevMissing { get; private set; }
        public int ImageIndexDependencyOptionalMissing { get; private set; }
        public int ImageIndexDependencyBundledMissing { get; private set; }

        public override Guid SharedCommandGuid {
            get {
                return Guids.NodejsCmdSet;
            }
        }

        protected override void FinishProjectCreation(string sourceFolder, string destFolder) {
            foreach (MSBuild.ProjectItem item in this.BuildProject.Items) {
                if (String.Equals(Path.GetExtension(item.EvaluatedInclude), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {

                    // Create the 'typings' folder
                    var typingsFolder = Path.Combine(ProjectHome, "Scripts", "typings");
                    if (!Directory.Exists(typingsFolder)) {
                        Directory.CreateDirectory(typingsFolder);
                    }

                    // Deploy node.d.ts
                    var nodeTypingsFolder = Path.Combine(typingsFolder, "node");
                    if (!Directory.Exists(Path.Combine(nodeTypingsFolder))) {
                        Directory.CreateDirectory(nodeTypingsFolder);
                    }

                    var nodeFolder = ((OAProject)this.GetAutomationObject()).ProjectItems
                        .AddFolder("Scripts").ProjectItems
                        .AddFolder("typings").ProjectItems
                        .AddFolder("node");

                    nodeFolder.ProjectItems.AddFromFileCopy(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "Scripts",
                            "typings",
                            "node",
                            "node.d.ts"
                        )
                    );
                    break;
                }
            }

            base.FinishProjectCreation(sourceFolder, destFolder);
        }

        protected override void AddNewFileNodeToHierarchy(HierarchyNode parentNode, string fileName) {
            base.AddNewFileNodeToHierarchy(parentNode, fileName);

            if (String.Equals(Path.GetExtension(fileName), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(GetProjectProperty(NodejsConstants.EnableTypeScript), "true", StringComparison.OrdinalIgnoreCase)) {
                // enable type script on the project automatically...
                SetProjectProperty(NodejsConstants.EnableTypeScript, "true");
                SetProjectProperty(NodejsConstants.TypeScriptSourceMap, "true");
                if (String.IsNullOrWhiteSpace(GetProjectProperty(NodejsConstants.TypeScriptModuleKind))) {
                    SetProjectProperty(NodejsConstants.TypeScriptModuleKind, NodejsConstants.CommonJSModuleKind);
                }
            }
        }

        internal static bool IsNodejsFile(string strFileName) {
            var ext = Path.GetExtension(strFileName);

            return String.Equals(ext, NodejsConstants.FileExtension, StringComparison.OrdinalIgnoreCase);
        }

        protected override string GetItemType(string filename) {
            if (string.Equals(Path.GetExtension(filename), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                return NodejsConstants.TypeScriptCompileItemType;
            }
            return base.GetItemType(filename);
        }

        protected override bool DisableCmdInCurrentMode(Guid commandGroup, uint command) {
            if (commandGroup == Guids.OfficeToolsBootstrapperCmdSet) {
                // Convert to ... commands from Office Tools don't make sense and aren't supported 
                // on our project type
                const int AddOfficeAppProject = 0x0001;
                const int AddSharePointAppProject = 0x0002;

                if (command == AddOfficeAppProject || command == AddSharePointAppProject) {
                    return true;
                }
            }

            return base.DisableCmdInCurrentMode(commandGroup, command);
        }

        public override string[] CodeFileExtensions {
            get {
                return new[] { NodejsConstants.FileExtension };
            }
        }

        protected internal override FolderNode CreateFolderNode(ProjectElement element) {
            return new CommonFolderNode(this, element);
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item) {
            var res = new NodejsFileNode(this, item);
            return res;
        }

        public override CommonFileNode CreateNonCodeFileNode(ProjectElement item) {
            string fileName = item.Url;
            if (!String.IsNullOrWhiteSpace(fileName)
                && Path.GetExtension(fileName).Equals(".ts", StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith(".d.ts",StringComparison.OrdinalIgnoreCase)) {
                return new NodejsTypeScriptFileNode(this, item);
            }
            if (Path.GetFileName(fileName).Equals(NodejsConstants.PackageJsonFile, StringComparison.OrdinalIgnoreCase)) {
                return new PackageJsonFileNode(this, item);
            }

            return base.CreateNonCodeFileNode(item);
        }

        public override string GetProjectName() {
            return "NodeProject";
        }

        public override Type GetProjectFactoryType() {
            return typeof(BaseNodeProjectFactory);
        }

        public override Type GetEditorFactoryType() {
            // Not presently used
            throw new NotImplementedException();
        }

        public override string GetFormatList() {
            return NodejsConstants.ProjectFileFilter;
        }

        protected override Guid[] GetConfigurationDependentPropertyPages() {
            var res = base.GetConfigurationDependentPropertyPages();

            var enableTs = GetProjectProperty(NodejsConstants.EnableTypeScript, false);
            bool fEnableTs;
            if (enableTs != null && Boolean.TryParse(enableTs, out fEnableTs) && fEnableTs) {
                var typeScriptPages = GetProjectProperty(NodejsConstants.TypeScriptCfgProperty);
                if (typeScriptPages != null) {
                    foreach (var strGuid in typeScriptPages.Split(';')) {
                        Guid guid;
                        if (Guid.TryParse(strGuid, out guid)) {
                            res = res.Append(guid);
                        }
                    }
                }
            }

            return res;
        }

        public override Type GetGeneralPropertyPageType() {
            return typeof(NodejsGeneralPropertyPage);
        }

        public override Type GetLibraryManagerType() {
            return typeof(NodejsLibraryManager);
        }

        public override IProjectLauncher GetLauncher() {
            return new NodejsProjectLauncher(this);
        }

        protected override NodeProperties CreatePropertiesObject() {
            return new NodejsProjectNodeProperties(this);
        }

        protected override Stream ProjectIconsImageStripStream {
            get {
                return typeof(ProjectNode).Assembly.GetManifestResourceStream("Microsoft.VisualStudioTools.Resources.Icons.SharedProjectImageList.bmp");
            }
        }

        public override bool IsCodeFile(string fileName) {
            return Path.GetExtension(fileName).Equals(".js", StringComparison.OrdinalIgnoreCase);
        }

        protected override void Reload() {
            using (new DebugTimer("Project Load")) {
                _intermediateOutputPath = Path.Combine(ProjectHome, GetProjectProperty("BaseIntermediateOutputPath"));

                if (_analyzer != null && _analyzer.RemoveUser()) {
                    _analyzer.Dispose();
                }
                _analyzer = new VsProjectAnalyzer(ProjectHome);

                base.Reload();

                SyncFileSystem();                

                NodejsPackage.Instance.CheckSurveyNews(false);
                ModulesNode.ReloadHierarchySafe();

                // scan for files which were loaded from cached analysis but no longer
                // exist and remove them.
                foreach (var module in _analyzer.Project.AllModules) {
                    if (Path.IsPathRooted(module.FilePath)) {   // ignore built-in modules
                        var treeNode = FindNodeByFullPath(module.FilePath);
                        if (treeNode == null) {
                            _analyzer.UnloadFile(module);
                        }
                    }
                }
            }
        }

        protected override void RaiseProjectPropertyChanged(string propertyName, string oldValue, string newValue) {
            base.RaiseProjectPropertyChanged(propertyName, oldValue, newValue);

            var propPage = GeneralPropertyPageControl;
            if (propPage != null) {
                switch (propertyName) {
                    case NodejsConstants.EnvironmentVariables:
                        propPage.EnvironmentVariables = newValue;
                        break;
                    case NodejsConstants.DebuggerPort:
                        propPage.DebuggerPort = newValue;
                        break;
                    case NodejsConstants.NodejsPort:
                        propPage.NodejsPort = newValue;
                        break;
                    case NodejsConstants.NodeExePath:
                        propPage.NodeExePath = newValue;
                        break;
                    case NodejsConstants.NodeExeArguments:
                        propPage.NodeExeArguments = newValue;
                        break;
                    case NodejsConstants.ScriptArguments:
                        propPage.ScriptArguments = newValue;
                        break;
                    case NodejsConstants.LaunchUrl:
                        propPage.LaunchUrl = newValue;
                        break;
                    case NodejsConstants.StartWebBrowser:
                        bool value;
                        if (Boolean.TryParse(newValue, out value)) {
                            propPage.StartWebBrowser = value;
                        }
                        break;
                    case CommonConstants.WorkingDirectory:
                        propPage.WorkingDirectory = newValue;
                        break;
                    default:
                        if (propPage != null) {
                            PropertyPage.IsDirty = true;
                        }
                        break;
                }
            }
        }

        private NodejsGeneralPropertyPageControl GeneralPropertyPageControl {
            get {
                if (PropertyPage != null && PropertyPage.Control != null) {
                    return (NodejsGeneralPropertyPageControl)PropertyPage.Control;
                }

                return null;
            }
        }

        private static void AddFolderForFile(Dictionary<FileNode, List<CommonFolderNode>> directoryPackages, FileNode rootFile, CommonFolderNode folderChild) {
            List<CommonFolderNode> folders;
            if (!directoryPackages.TryGetValue(rootFile, out folders)) {
                directoryPackages[rootFile] = folders = new List<CommonFolderNode>();
            }
            folders.Add(folderChild);
        }

        protected override bool IncludeNonMemberItemInProject(HierarchyNode node) {
            var fileNode = node as NodejsFileNode;
            if (fileNode != null) {
                return IncludeNodejsFile(fileNode);
            }
            return false;
        }

        internal bool IncludeNodejsFile(NodejsFileNode fileNode) {
            if (CommonUtils.IsSubpathOf(_intermediateOutputPath, fileNode.Url)) {
                return false;
            }
            return true;
        }

        internal override object Object {
            get {
                return this;
            }
        }

        protected override ReferenceContainerNode CreateReferenceContainerNode() {
            return null;
        }

        public NodeModulesNode ModulesNode { get; private set; }



        protected internal override void ProcessReferences() {
            base.ProcessReferences();

            if (null == ModulesNode) {
                ModulesNode = new NodeModulesNode(this);
                AddChild(ModulesNode);
            }
        }

        #region VSWebSite Members

        // This interface is just implemented so we don't get normal profiling which
        // doesn't work with our projects anyway.

        public EnvDTE.ProjectItem AddFromTemplate(string bstrRelFolderUrl, string bstrWizardName, string bstrLanguage, string bstrItemName, bool bUseCodeSeparation, string bstrMasterPage, string bstrDocType) {
            throw new NotImplementedException();
        }

        public VsWebSite.CodeFolders CodeFolders {
            get { throw new NotImplementedException(); }
        }

        public EnvDTE.DTE DTE {
            get { return Project.DTE; }
        }

        public string EnsureServerRunning() {
            throw new NotImplementedException();
        }

        public string GetUniqueFilename(string bstrFolder, string bstrRoot, string bstrDesiredExt) {
            throw new NotImplementedException();
        }

        public bool PreCompileWeb(string bstrCompilePath, bool bUpdateable) {
            throw new NotImplementedException();
        }

        public EnvDTE.Project Project {
            get { return (OAProject)GetAutomationObject(); }
        }

        public VsWebSite.AssemblyReferences References {
            get { throw new NotImplementedException(); }
        }

        public void Refresh() {
        }

        public string TemplatePath {
            get { throw new NotImplementedException(); }
        }

        public string URL {
            get { throw new NotImplementedException(); }
        }

        public string UserTemplatePath {
            get { throw new NotImplementedException(); }
        }

        public VsWebSite.VSWebSiteEvents VSWebSiteEvents {
            get { throw new NotImplementedException(); }
        }

        public void WaitUntilReady() {
        }

        public VsWebSite.WebReferences WebReferences {
            get { throw new NotImplementedException(); }
        }

        public VsWebSite.WebServices WebServices {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public Task InstallMissingModules() {
            //Fire off the command to update the missing modules
            //  through NPM
            return ModulesNode.InstallMissingModules();
        }

        private void HookErrorsAndWarnings(VsProjectAnalyzer res) {
            res.ErrorAdded += OnErrorAdded;
            res.ErrorRemoved += OnErrorRemoved;
            res.WarningAdded += OnWarningAdded;
            res.WarningRemoved += OnWarningRemoved;
        }

        private void UnHookErrorsAndWarnings(VsProjectAnalyzer res) {
            res.ErrorAdded -= OnErrorAdded;
            res.ErrorRemoved -= OnErrorRemoved;
            res.WarningAdded -= OnWarningAdded;
            res.WarningRemoved -= OnWarningRemoved;
        }

        private void OnErrorAdded(object sender, FileEventArgs args) {
            if (_diskNodes.ContainsKey(args.Filename)) {
                _errorFiles.Add(args.Filename);
            }
        }

        private void OnErrorRemoved(object sender, FileEventArgs args) {
            _errorFiles.Remove(args.Filename);
        }

        private void OnWarningAdded(object sender, FileEventArgs args) {
            if (_diskNodes.ContainsKey(args.Filename)) {
                _warningFiles.Add(args.Filename);
            }
        }

        private void OnWarningRemoved(object sender, FileEventArgs args) {
            _warningFiles.Remove(args.Filename);
        }

        /// <summary>
        /// File names within the project which contain errors.
        /// </summary>
        public HashSet<string> ErrorFiles {
            get {
                return _errorFiles;
            }
        }

        /// <summary>
        /// File names within the project which contain warnings.
        /// </summary>
        public HashSet<string> WarningFiles {
            get {
                return _warningFiles;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                UnHookErrorsAndWarnings(_analyzer);
                if (WarningFiles.Count > 0 || ErrorFiles.Count > 0) {
                    foreach (var file in WarningFiles.Concat(ErrorFiles)) {
                        var node = FindNodeByFullPath(file) as NodejsFileNode;
                        if (node != null) {
                            //_analyzer.RemoveErrors(node.GetAnalysis(), suppressUpdate: false);
                        }
                    }
                }

                if (_analyzer.RemoveUser()) {
                    _analyzer.Dispose();
                }
                _analyzer = null;
            }
            base.Dispose(disposing);
        }
    }
}
