//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.ProjectWizard;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;
using MSBuild = Microsoft.Build.Evaluation;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
#if DEV14_OR_LATER
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
#endif

namespace Microsoft.NodejsTools.Project {
    class NodejsProjectNode : CommonProjectNode, VsWebSite.VSWebSite, INodePackageModulesCommands, IVsBuildPropertyStorage {
        private VsProjectAnalyzer _analyzer;
        private readonly HashSet<string> _warningFiles = new HashSet<string>();
        private readonly HashSet<string> _errorFiles = new HashSet<string>();
        private string[] _analysisIgnoredDirs = new string[1] { NodejsConstants.NodeModulesStagingFolder };
        private int _maxFileSize = 1024 * 512;
        internal readonly RequireCompletionCache _requireCompletionCache = new RequireCompletionCache();
        private string _intermediateOutputPath;
        private readonly Dictionary<NodejsProjectImageName, int> _imageIndexFromNameDictionary = new Dictionary<NodejsProjectImageName, int>();

        // We delay analysis until things calm down in the node_modules folder.
        internal Queue<NodejsFileNode> DelayedAnalysisQueue = new Queue<NodejsFileNode>();
        private object _idleNodeModulesLock = new object();
        private volatile bool _isIdleNodeModules = false;
        private Timer _idleNodeModulesTimer;

        public NodejsProjectNode(NodejsProjectPackage package)
            : base(
                  package,
#if DEV14_OR_LATER
                  null
#else
                  Utilities.GetImageList(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.Resources.Icons.NodejsImageList.bmp"))
#endif
        ) {
            Type projectNodePropsType = typeof(NodejsProjectNodeProperties);
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
#pragma warning disable 0612
            InitNodejsProjectImages();
#pragma warning restore 0612

        }

        public VsProjectAnalyzer Analyzer {
            get {
                return _analyzer;
            }
        }

        private void OnIdleNodeModules(object state) {
            lock (_idleNodeModulesLock) {
                _isIdleNodeModules = true;
            }

            while (DelayedAnalysisQueue.Count > 0) {
                lock (_idleNodeModulesLock) {
                    if (!_isIdleNodeModules) {
                        return;
                    }
                }
                var fileNode = DelayedAnalysisQueue.Dequeue();
                if (fileNode != null) {
                    fileNode.Analyze();
                }
            }
        }

        internal void EnqueueForDelayedAnalysis(NodejsFileNode fileNode) {
            DelayedAnalysisQueue.Enqueue(fileNode);
            RestartIdleNodeModulesTimer();
        }

        private void RestartIdleNodeModulesTimer() {
            lock (_idleNodeModulesLock) {
                _isIdleNodeModules = false;

                // The cooldown time here is longer than the cooldown time we use in NpmController.
                // This gives the Npm component ample time to build up the npm node tree,
                // so that we can query it later for perf optimizations.
                if (_idleNodeModulesTimer != null) {
                    _idleNodeModulesTimer.Change(3000, Timeout.Infinite);
                }
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

        public Dictionary<NodejsProjectImageName, int> ImageIndexFromNameDictionary {
            get { return _imageIndexFromNameDictionary; }
        }

#if DEV14_OR_LATER
        [Obsolete]
#endif
        private void InitNodejsProjectImages() {
            // HACK: https://nodejstools.codeplex.com/workitem/1268

            // Project file images
            AddProjectImage(NodejsProjectImageName.TypeScriptProjectFile, "Microsoft.VisualStudioTools.Resources.Icons.TSProject_SolutionExplorerNode.png");

            // Dependency images
            AddProjectImage(NodejsProjectImageName.Dependency, "Microsoft.VisualStudioTools.Resources.Icons.NodeJSPackage_16x.png");
            AddProjectImage(NodejsProjectImageName.DependencyNotListed, "Microsoft.VisualStudioTools.Resources.Icons.NodeJSPackageMissing_16x.png");
            AddProjectImage(NodejsProjectImageName.DependencyMissing, "Microsoft.VisualStudioTools.Resources.Icons.PackageWarning_16x.png");
        }

#if DEV14_OR_LATER
        protected override bool SupportsIconMonikers {
            get { return true; }
        }

        protected override ImageMoniker GetIconMoniker(bool open) {
            if (string.Equals(GetProjectProperty(NodejsConstants.EnableTypeScript), "true", StringComparison.OrdinalIgnoreCase)) {
                return KnownMonikers.TSProjectNode;
            }
            return KnownMonikers.JSProjectNode;
        }

        [Obsolete]
#endif
        private void AddProjectImage(NodejsProjectImageName name, string resourceId) {
            var images = ImageHandler.ImageList.Images;
            ImageIndexFromNameDictionary.Add(name, images.Count);
            images.Add(Image.FromStream(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream(resourceId)));
        }

        public override Guid SharedCommandGuid {
            get {
                return Guids.NodejsCmdSet;
            }
        }

#if !DEV14_OR_LATER  
        public override int ImageIndex {
            get {
                if (string.Equals(GetProjectProperty(NodejsConstants.EnableTypeScript), "true", StringComparison.OrdinalIgnoreCase)) {
                    return ImageIndexFromNameDictionary[NodejsProjectImageName.TypeScriptProjectFile];
                }
                return base.ImageIndex;
            }
        }
#endif
        internal override string IssueTrackerUrl {
            get { return NodejsConstants.IssueTrackerUrl; }
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

            return String.Equals(ext, NodejsConstants.JavaScriptExtension, StringComparison.OrdinalIgnoreCase);
        }

        internal override string GetItemType(string filename) {
            string absFileName =
                Path.IsPathRooted(filename) ?
                filename :
                Path.Combine(this.ProjectHome, filename);

            var node = this.FindNodeByFullPath(absFileName) as NodejsFileNode;
            if (node != null && node.ItemNode.ItemTypeName != null) {
                return node.ItemNode.ItemTypeName;
            }

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

            if (commandGroup == VSConstants.GUID_VSStandardCommandSet97) {
                if (this.IsCurrentStateASuppressCommandsMode()) {
                    switch ((VsCommands)command) {
                        default:
                            break;
                        case VsCommands.UnloadProject:
                        case VsCommands.NewFolder:
                        case VsCommands.EditLabel:
                        case VsCommands.Rename:
                            return true;
                    }
                }
            }

            // don't defer to base class, Node allows edits while debugging (adding new files, etc...)
            return false;
        }

        public override string[] CodeFileExtensions {
            get {
                return new[] { NodejsConstants.JavaScriptExtension };
            }
        }

        protected internal override FolderNode CreateFolderNode(ProjectElement element) {
            return new NodejsFolderNode(this, element);
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item) {
            string fileName = item.Url;
            if (!String.IsNullOrWhiteSpace(fileName)
                && Path.GetExtension(fileName).Equals(NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                return new NodejsTypeScriptFileNode(this, item);
            }
            var res = new NodejsFileNode(this, item);
            return res;
        }

        public override CommonFileNode CreateNonCodeFileNode(ProjectElement item) {
            string fileName = item.Url;
            if (Path.GetFileName(fileName).Equals(NodejsConstants.PackageJsonFile, StringComparison.OrdinalIgnoreCase) && 
                !fileName.Contains(NodejsConstants.NodeModulesStagingFolder)) {
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

            var enableTs = GetProjectProperty(NodejsConstants.EnableTypeScript, resetCache: false);
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
            var ext = Path.GetExtension(fileName);
            return ext.Equals(NodejsConstants.JavaScriptExtension, StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase);
        }

        public override int InitializeForOuter(string filename, string location, string name, uint flags, ref Guid iid, out IntPtr projectPointer, out int canceled) {
            NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLevelChanged += IntellisenseOptionsPageAnalysisLevelChanged;
            NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLogMaximumChanged += AnalysisLogMaximumChanged;
            NodejsPackage.Instance.IntellisenseOptionsPage.SaveToDiskChanged += IntellisenseOptionsPageSaveToDiskChanged;
            NodejsPackage.Instance.GeneralOptionsPage.ShowBrowserAndNodeLabelsChanged += ShowBrowserAndNodeLabelsChanged;

            return base.InitializeForOuter(filename, location, name, flags, ref iid, out projectPointer, out canceled);
        }

        protected override void Reload() {
            using (new DebugTimer("Project Load")) {
                // Populate values from project properties before we do anything else.
                // Otherwise we run into race conditions where, for instance, _analysisIgnoredDirectories
                // is not properly set before the FileNodes get created in base.Reload()
                UpdateProjectNodeFromProjectProperties();

                if (_analyzer != null && _analyzer.RemoveUser()) {
                    _analyzer.Dispose();
                }
                _analyzer = new VsProjectAnalyzer(ProjectFolder);
                _analyzer.MaxLogLength = NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLogMax;
                LogAnalysisLevel();

                base.Reload();

                SyncFileSystem();

                NodejsPackage.Instance.CheckSurveyNews(false);
                ModulesNode.ReloadHierarchySafe();

                // scan for files which were loaded from cached analysis but no longer
                // exist and remove them.
                _analyzer.ReloadComplete();
            }
        }

        private void UpdateProjectNodeFromProjectProperties() {
            _intermediateOutputPath = Path.Combine(ProjectHome, GetProjectProperty("BaseIntermediateOutputPath"));

            var ignoredPaths = GetProjectProperty(NodejsConstants.AnalysisIgnoredDirectories);

            if (!string.IsNullOrWhiteSpace(ignoredPaths)) {
                _analysisIgnoredDirs = _analysisIgnoredDirs.Append(ignoredPaths.Split(';').Select(x => '\\' + x + '\\').ToArray());
            }

            var maxFileSizeProp = GetProjectProperty(NodejsConstants.AnalysisMaxFileSize);
            int maxFileSize;
            if (maxFileSizeProp != null && Int32.TryParse(maxFileSizeProp, out maxFileSize)) {
                _maxFileSize = maxFileSize;
            }
        }

        private void Reanalyze(HierarchyNode node, VsProjectAnalyzer newAnalyzer) {
            if (node != null) {
                for (var child = node.FirstChild; child != null; child = child.NextSibling) {
                    if (child is PackageJsonFileNode) {
                        ((PackageJsonFileNode)child).AnalyzePackageJson(newAnalyzer);
                    } else if (child is NodejsFileNode) {
                        if (((NodejsFileNode)child).ShouldAnalyze) {
                            newAnalyzer.AnalyzeFile(child.Url, !child.IsNonMemberItem);
                        }
                    }

                    Reanalyze(child, newAnalyzer);
                }
            }
        }

        private void LogAnalysisLevel() {
            var analyzer = _analyzer;
            if (analyzer != null) {
                NodejsPackage.Instance.Logger.LogEvent(Logging.NodejsToolsLogEvent.AnalysisLevel, (int)analyzer.AnalysisLevel);
            }
        }

        /*
         * Needed if we switch to per project Analysis levels
        internal NodejsTools.Options.AnalysisLevel AnalysisLevel(){
            var analyzer = _analyzer;
            if (_analyzer != null) {
                return _analyzer.AnalysisLevel;
            }
            return NodejsTools.Options.AnalysisLevel.None;
        }
        */
        private void IntellisenseOptionsPageAnalysisLevelChanged(object sender, EventArgs e) {

            var oldAnalyzer = _analyzer;
            _analyzer = null;

            var analyzer = new VsProjectAnalyzer(ProjectFolder);
            Reanalyze(this, analyzer);
            if (oldAnalyzer != null) {
                analyzer.SwitchAnalyzers(oldAnalyzer);
                if (oldAnalyzer.RemoveUser()) {
                    oldAnalyzer.Dispose();
                }
            }
            _analyzer = analyzer;
            _analyzer.MaxLogLength = NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLogMax;
            LogAnalysisLevel();
        }

        private void AnalysisLogMaximumChanged(object sender, EventArgs e) {
            if (_analyzer != null) {
                _analyzer.MaxLogLength = NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLogMax;
            }
        }

        private void IntellisenseOptionsPageSaveToDiskChanged(object sender, EventArgs e) {
            if (_analyzer != null) {
                _analyzer.SaveToDisk = NodejsPackage.Instance.IntellisenseOptionsPage.SaveToDisk;
            }
        }

        private void ShowBrowserAndNodeLabelsChanged(object sender, EventArgs e) {
            var nodejsFolderNodes = this.AllDescendants.Where(item => (item as NodejsFolderNode) != null).Select(item => (NodejsFolderNode)item);
            foreach (var node in nodejsFolderNodes) {
                ProjectMgr.ReDrawNode(node, UIHierarchyElement.Caption);
            }
        }

        protected override void RaiseProjectPropertyChanged(string propertyName, string oldValue, string newValue) {
            base.RaiseProjectPropertyChanged(propertyName, oldValue, newValue);

            var propPage = GeneralPropertyPageControl;
            if (propPage != null) {
                switch (propertyName) {
                    case NodejsConstants.Environment:
                        propPage.Environment = newValue;
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
                    case CommonConstants.StartupFile:
                        propPage.ScriptFile = newValue;
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
            var url = fileNode.Url;
            if (CommonUtils.IsSubpathOf(_intermediateOutputPath, fileNode.Url)) {
                return false;
            }

            foreach (var path in _analysisIgnoredDirs) {
                if (url.IndexOf(path, 0, StringComparison.OrdinalIgnoreCase) != -1) {
                    return false;
                }
            }

            var fileInfo = new FileInfo(fileNode.Url);
            if (!fileInfo.Exists || fileInfo.Length > _maxFileSize) {
                // skip obviously generated and missing files...
                return false;
            }

            int nestedModulesDepth = 0;
            if (ModulesNode.NpmController.RootPackage != null && ModulesNode.NpmController.RootPackage.Modules != null) {
                nestedModulesDepth = ModulesNode.NpmController.RootPackage.Modules.GetDepth(fileNode.Url);
            }

            if (_analyzer != null && _analyzer.Project != null &&
                _analyzer.Project.Limits.IsPathExceedNestingLimit(nestedModulesDepth)) {
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
                _idleNodeModulesTimer = new Timer(OnIdleNodeModules);
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

        Task INodePackageModulesCommands.InstallMissingModulesAsync() {
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

        internal struct LongPathInfo {
            public string FullPath;
            public string RelativePath;
            public bool IsDirectory;
        }

        private static readonly Regex _uninstallRegex = new Regex(@"\b(uninstall|rm)\b");
        private static readonly char[] _pathSeparators = { '\\', '/' };
        private bool _isCheckingForLongPaths;

        public async Task CheckForLongPaths(string npmArguments = null) {
            if (_isCheckingForLongPaths || !NodejsPackage.Instance.GeneralOptionsPage.CheckForLongPaths) {
                return;
            }

            if (npmArguments != null && _uninstallRegex.IsMatch(npmArguments)) {
                return;
            }

            try {
                _isCheckingForLongPaths = true;
                TaskDialogButton dedupeButton, ignoreButton, disableButton;
                var taskDialog = new TaskDialog(NodejsPackage.Instance) {
                    AllowCancellation = true,
                    EnableHyperlinks = true,
                    Title = SR.GetString(SR.LongPathWarningTitle),
                    MainIcon = TaskDialogIcon.Warning,
                    Content = SR.GetString(SR.LongPathWarningText),
                    CollapsedControlText = SR.GetString(SR.LongPathShowPathsExceedingTheLimit),
                    ExpandedControlText = SR.GetString(SR.LongPathHidePathsExceedingTheLimit),
                    Buttons = {
                        (dedupeButton = new TaskDialogButton(SR.GetString(SR.LongPathNpmDedupe), SR.GetString(SR.LongPathNpmDedupeDetail))),
                        (ignoreButton = new TaskDialogButton(SR.GetString(SR.LongPathDoNothingButWarnNextTime))),
                        (disableButton = new TaskDialogButton(SR.GetString(SR.LongPathDoNothingAndDoNotWarnAgain), SR.GetString(SR.LongPathDoNothingAndDoNotWarnAgainDetail)))
                    },
                    FooterIcon = TaskDialogIcon.Information,
                    Footer = SR.GetString(SR.LongPathFooter)
                };

                taskDialog.HyperlinkClicked += (sender, e) => {
                    switch (e.Url) {
                        case "#msdn":
                            Process.Start("http://go.microsoft.com/fwlink/?LinkId=454508");
                            break;
                        case "#uservoice":
                            Process.Start("http://go.microsoft.com/fwlink/?LinkID=456509");
                            break;
                        case "#help":
                            Process.Start("http://go.microsoft.com/fwlink/?LinkId=456511");
                            break;
                        default:
                            System.Windows.Clipboard.SetText(e.Url);
                            break;
                    }
                };

                recheck:

                var longPaths = await Task.Factory.StartNew(() =>
                    GetLongSubPaths(ProjectHome)
                    .Concat(GetLongSubPaths(_intermediateOutputPath))
                    .Select(lpi => string.Format("• {1}\u00A0<a href=\"{0}\">{2}</a>", lpi.FullPath, lpi.RelativePath, SR.GetString(SR.LongPathClickToCopy)))
                    .ToArray());
                if (longPaths.Length == 0) {
                    return;
                }
                taskDialog.ExpandedInformation = string.Join("\r\n", longPaths);

                var button = taskDialog.ShowModal();
                if (button == dedupeButton) {
                    var repl = NodejsPackage.Instance.OpenReplWindow(focus: false);
                    await repl.ExecuteCommand(".npm dedupe").HandleAllExceptions(SR.ProductName);

                    taskDialog.Content += "\r\n\r\n" + SR.GetString(SR.LongPathNpmDedupeDidNotHelp);
                    taskDialog.Buttons.Remove(dedupeButton);
                    goto recheck;
                } else if (button == disableButton) {
                    var page = NodejsPackage.Instance.GeneralOptionsPage;
                    page.CheckForLongPaths = false;
                    page.SaveSettingsToStorage();
                }
            } finally {
                _isCheckingForLongPaths = false;
            }
        }

        internal static IEnumerable<LongPathInfo> GetLongSubPaths(string basePath, string path = "") {
            const int MaxFilePathLength = 260 - 1; // account for terminating NULL
            const int MaxDirectoryPathLength = 248 - 1;

            basePath = CommonUtils.EnsureEndSeparator(basePath);

            WIN32_FIND_DATA wfd;
            IntPtr hFind = NativeMethods.FindFirstFile(basePath + path + "\\*", out wfd);
            if (hFind == NativeMethods.INVALID_HANDLE_VALUE) {
                yield break;
            }

            try {
                do {
                    if (wfd.cFileName == "." || wfd.cFileName == "..") {
                        continue;
                    }

                    bool isDirectory = (wfd.dwFileAttributes & NativeMethods.FILE_ATTRIBUTE_DIRECTORY) != 0;

                    string childPath = path;
                    if (childPath != String.Empty) {
                        childPath += "\\";
                    }
                    childPath += wfd.cFileName;

                    string fullChildPath = basePath + childPath;
                    bool isTooLong;
                    try {
                        isTooLong = Path.GetFullPath(fullChildPath).Length > (isDirectory ? MaxDirectoryPathLength : MaxFilePathLength);
                    } catch (PathTooLongException) {
                        isTooLong = true;
                    } catch (Exception) {
                        continue;
                    }

                    if (isTooLong) {
                        yield return new LongPathInfo { FullPath = fullChildPath, RelativePath = childPath, IsDirectory = isDirectory };
                    } else if (isDirectory) {
                        foreach (var item in GetLongSubPaths(basePath, childPath)) {
                            yield return item;
                        }
                    }
                } while (NativeMethods.FindNextFile(hFind, out wfd));
            } finally {
                NativeMethods.FindClose(hFind);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_analyzer != null) {
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

                lock (_idleNodeModulesLock) {
                    if (_idleNodeModulesTimer != null) {
                        _idleNodeModulesTimer.Dispose();
                    }
                    _idleNodeModulesTimer = null;
                }

                NodejsPackage.Instance.IntellisenseOptionsPage.SaveToDiskChanged -= IntellisenseOptionsPageSaveToDiskChanged;
                NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLevelChanged -= IntellisenseOptionsPageAnalysisLevelChanged;
                NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLogMaximumChanged -= AnalysisLogMaximumChanged;
            }
            base.Dispose(disposing);
        }

        internal override async void BuildAsync(uint vsopts, string config, VisualStudio.Shell.Interop.IVsOutputWindowPane output, string target, Action<MSBuildResult, string> uiThreadCallback) {
            try {
                await CheckForLongPaths();
            } catch (Exception) {
                uiThreadCallback(MSBuildResult.Failed, target);
                return;
            }

            // BuildAsync can throw on the sync path before invoking the callback. If it does, we must still invoke the callback here,
            // because by this time there's no other way to propagate the error to the caller.
            try {
                base.BuildAsync(vsopts, config, output, target, uiThreadCallback);
            } catch (Exception) {
                uiThreadCallback(MSBuildResult.Failed, target);
            }
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidOpenReplWindow:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        protected override QueryStatusResult QueryStatusSelectionOnNodes(IList<HierarchyNode> selectedNodes, Guid cmdGroup, uint cmd, IntPtr pCmdText) {
            if (cmdGroup == Guids.NodejsNpmCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidNpmManageModules:
                        if (IsCurrentStateASuppressCommandsMode()) {
                            return QueryStatusResult.SUPPORTED;
                        } else if (!ShowManageModulesCommandOnNode(selectedNodes)) {
                            return QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED | QueryStatusResult.INVISIBLE;
                        }
                        return QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                }
            } else if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidSetAsNodejsStartupFile:
                        if (ShowSetAsStartupFileCommandOnNode(selectedNodes)) {
                            // We enable "Set as StartUp File" command only on current language code files, 
                            // the file is in project home dir and if the file is not the startup file already.
                            string startupFile = ((CommonProjectNode)ProjectMgr).GetStartupFile();
                            if (!CommonUtils.IsSamePath(startupFile, selectedNodes[0].Url)) {
                                return QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            }
                        }
                        break;
                }
            }

            return base.QueryStatusSelectionOnNodes(selectedNodes, cmdGroup, cmd, pCmdText);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidOpenReplWindow:
                        NodejsPackage.Instance.OpenReplWindow();
                        return VSConstants.S_OK;
                }
            } else if (cmdGroup == Guids.NodejsNpmCmdSet) {
                try {
                    NpmHelpers.GetPathToNpm(
                        Nodejs.GetAbsoluteNodeExePath(
                            ProjectHome,
                            Project.GetNodejsProject().GetProjectProperty(NodejsConstants.NodeExePath)
                    ));
                } catch (NpmNotFoundException) {
                    Nodejs.ShowNodejsNotInstalled();
                    return VSConstants.S_OK;
                }
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override int ExecCommandThatDependsOnSelectedNodes(Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr vaIn, IntPtr vaOut, CommandOrigin commandOrigin, IList<HierarchyNode> selectedNodes, out bool handled) {
            if (cmdGroup == Guids.NodejsNpmCmdSet) {
                try {
                    NpmHelpers.GetPathToNpm(
                        Nodejs.GetAbsoluteNodeExePath(
                            ProjectHome,
                            Project.GetNodejsProject().GetProjectProperty(NodejsConstants.NodeExePath)
                    ));
                } catch (NpmNotFoundException) {
                    Nodejs.ShowNodejsNotInstalled();
                    handled = true;
                    return VSConstants.S_OK;
                }

                switch (cmdId) {
                    case PkgCmdId.cmdidNpmManageModules:
                        if (!ShowManageModulesCommandOnNode(selectedNodes)) {
                            ModulesNode.ManageModules();
                            handled = true;
                            return VSConstants.S_OK;
                        }

                        var node = selectedNodes[0] as AbstractNpmNode;
                        if (node != null) {
                            var abstractNpmNode = node;
                            abstractNpmNode.ManageNpmModules();
                            handled = true;
                            return VSConstants.S_OK;
                        }
                        break;
                }
            } else if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmdId) {
                    case PkgCmdId.cmdidSetAsNodejsStartupFile:
                        // Set the StartupFile project property to the Url of this node
                        SetProjectProperty(
                            CommonConstants.StartupFile,
                            CommonUtils.GetRelativeFilePath(ProjectHome, selectedNodes[0].Url)
                        );
                        handled = true;
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandThatDependsOnSelectedNodes(cmdGroup, cmdId, cmdExecOpt, vaIn, vaOut, commandOrigin, selectedNodes, out handled);
        }

        private bool ShowSetAsStartupFileCommandOnNode(IList<HierarchyNode> selectedNodes) {
            var selectedNodeUrl = selectedNodes[0].Url;
            return selectedNodes.Count == 1 &&
                (IsCodeFile(selectedNodeUrl) ||
                // for some reason, the default express 4 template's startup file lacks an extension.
                string.IsNullOrEmpty(Path.GetExtension(selectedNodeUrl)));
        }

        private static bool ShowManageModulesCommandOnNode(IList<HierarchyNode> selectedNodes) {
            return selectedNodes.Count == 1 && selectedNodes[0] is AbstractNpmNode;
        }

        protected internal override void SetCurrentConfiguration() {
            if (!IsProjectOpened) {
                return;
            }

            if (this.IsPlatformAware()) {
                EnvDTE.Project automationObject = GetAutomationObject() as EnvDTE.Project;

                this.BuildProject.SetGlobalProperty(ProjectFileConstants.Platform, automationObject.ConfigurationManager.ActiveConfiguration.PlatformName);
            }
            base.SetCurrentConfiguration();
        }

        public override MSBuildResult Build(string config, string target) {
            if (this.IsPlatformAware()) {
                var platform = this.BuildProject.GetPropertyValue(GlobalProperty.Platform.ToString());

                if (platform == ProjectConfig.AnyCPU) {
                    this.BuildProject.SetGlobalProperty(GlobalProperty.Platform.ToString(), ConfigProvider.x86Platform);
                }
            }
            return base.Build(config, target);
        }

    }
}
