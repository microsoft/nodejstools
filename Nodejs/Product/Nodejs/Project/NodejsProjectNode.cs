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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.NodejsTools.Project {
    class NodejsProjectNode : CommonProjectNode, VsWebSite.VSWebSite {
        private static string _nodeRefCode = ReadNodeRefCode();
        internal readonly string _referenceFilename = GetReferenceFilePath();
        const string _userSwitchMarker = "// **NTVS** INSERT USER MODULE SWITCH HERE **NTVS**";
        internal readonly List<NodejsFileNode> _nodeFiles = new List<NodejsFileNode>();
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();        
        private readonly Timer _timer;
        internal readonly ReferenceGroupDispenser _refGroupDispenser = new ReferenceGroupDispenser();
        private readonly HashSet<ReferenceGroup> _pendingRefGroupGenerations = new HashSet<ReferenceGroup>();
        internal readonly RequireCompletionCache _requireCompletionCache = new RequireCompletionCache();
        internal int _currentFileCounter;

        public NodejsProjectNode(NodejsProjectPackage package)
            : base(package, Utilities.GetImageList(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.NodeImageList.bmp"))) {

            Type projectNodePropsType = typeof(NodejsProjectNodeProperties);
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
            InitDependencyImages();
            _timer = new Timer(RefreshReferenceFile);
        }

        private void InitDependencyImages()
        {
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

        private void RefreshReferenceFile(object state) {
            UpdateReferenceFile();
        }

        public override int InitializeForOuter(string filename, string location, string name, uint flags, ref Guid iid, out IntPtr projectPointer, out int canceled) {
            int res = base.InitializeForOuter(filename, location, name, flags, ref iid, out projectPointer, out canceled);
            if (ErrorHandler.Succeeded(res)) {
                UpdateReferenceFile();
            }
            return res;
        }

        public override Guid SharedCommandGuid {
            get {
                return Guids.NodejsCmdSet;
            }
        }

        protected override void FinishProjectCreation(string sourceFolder, string destFolder) {
            foreach (MSBuild.ProjectItem item in this.BuildProject.Items) {
                if (String.Equals(Path.GetExtension(item.EvaluatedInclude), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                    // If we have a TypeScript project deploy our node reference file.
                    File.Copy(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "node.d.ts"
                        ),
                        Path.Combine(ProjectHome, "node.d.ts")
                    );

                    // copy any additional d.ts files
                    foreach (var file in Directory.EnumerateFiles(sourceFolder, "*.d.ts", SearchOption.AllDirectories)) {
                        var destPath = Path.Combine(
                            destFolder,
                            CommonUtils.GetRelativeFilePath(sourceFolder, file)
                        );
                        File.Copy(file, destPath);
                        new FileInfo(destPath).Attributes = FileAttributes.Normal;
                    }
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

        public override void Close() {
            base.Close();
            if (File.Exists(_referenceFilename)) {
                File.Delete(_referenceFilename);
            }
        }

        public override string[] CodeFileExtensions {
            get {
                return new[] { NodejsConstants.FileExtension };
            }
        }

        internal static string GetReferenceFilePath() {
            string res;
            do {
                // .js files instead of just using Path.GetTempPath because the JS
                // language service analyzes .js files.                
                res = Path.Combine(
                    Path.GetTempPath(),
                    Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".js"
                );
            } while (File.Exists(res));
            return res;
        }

        protected internal override FolderNode CreateFolderNode(ProjectElement element) {
            return new CommonFolderNode(this, element);
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item) {
            var res = new NodejsFileNode(this, item);

            if (ParentHierarchy != null) {
                lock (_pendingRefGroupGenerations) {
                    _pendingRefGroupGenerations.Add(res._refGroup);
                }
            }

            _timer.Change(250, Timeout.Infinite);
            return res;
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
                return typeof(ProjectNode).Assembly.GetManifestResourceStream("imagelis.bmp");
            }
        }

        public override bool IsCodeFile(string fileName) {
            return Path.GetExtension(fileName).Equals(".js", StringComparison.OrdinalIgnoreCase);
        }

        protected override void Reload() {
            using (new DebugTimer("Project Load")) {
                base.Reload();

                SyncFileSystem();

                foreach (var group in _refGroupDispenser.Groups) {
                    group.GenerateReferenceFile();
                }

                NodejsPackage.Instance.CheckSurveyNews(false);
                ModulesNode.ReloadHierarchySafe();
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

        /// <summary>
        /// Updates our per-project reference .js file.  This file gets our baseline node reference file
        /// merged with all of the user's code as well.  This allows the user to do require('mymodule') and
        /// get the correct intellisense back.
        /// </summary>
        internal void UpdateReferenceFile(NodejsFileNode changedFile = null) {
            // don't update if we're closing the project
            if (!IsClosed && !IsClosing) {
                _uiSync.BeginInvoke((Action<NodejsFileNode>)UpdateReferenceFileUIThread, changedFile);
            }
        }

        private void UpdateReferenceFileUIThread(NodejsFileNode changedFile) {            
            lock (_pendingRefGroupGenerations) {
                foreach (var refGroup in _pendingRefGroupGenerations) {
                    refGroup.GenerateReferenceFile();
                }
                _pendingRefGroupGenerations.Clear();
            }

            StringBuilder header = new StringBuilder();
            foreach (var refGroup in _refGroupDispenser.Groups) {
                header.AppendLine("/// <reference path=\"" + refGroup.Filename + "\" />");
            }

            StringBuilder switchCode = new StringBuilder();
            UpdateReferenceFile(this, switchCode);

            _requireCompletionCache.Clear();
            
            WriteReferenceFile(
                _referenceFilename, 
                header + _nodeRefCode.Replace(_userSwitchMarker, switchCode.ToString())
            );
        }

        internal static void WriteReferenceFile(string filename, string output) {
            for (int i = 0; i < 10; i++) {
                try {
                    File.WriteAllText(filename, output);
                    break;
                } catch (IOException) {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Walks the project and generates the reference code for each .js file present.
        /// </summary>
        private void UpdateReferenceFile(HierarchyNode node, StringBuilder switchCode) {
            // collect all of the node_modules folders
            Dictionary<FileNode, List<CommonFolderNode>> directoryPackages = GetModuleFolderMapping();

            switchCode.Append(@"
function relative_match(match_dir, dirname, mod_name, alt_mod_name, ref_path) {
     if(ref_path.substr(0, 3) == '../') {
        // fall into ./ case below
        ref_path = './' + ref_path;
     }
     if(ref_path.substr(0, 2) == './') {
         var components = ref_path.split('/');
         var upCount = 0;

        // loops are unrolled here so that the JavaScript language service will always fully process
        // them, otherwise it may stop executing them if execution takes too long, and then
        // we get incorrect results.
         if(components.length >= 2 && components[1] == '..') {
            upCount++;
         }
         if(components.length >= 3 && components[2] == '..') {
            upCount++;
         }
         if(components.length >= 4 && components[3] == '..') {
            upCount++;
         }
         if(components.length >= 5 && components[4] == '..') {
            upCount++;
         }
         for(var i = 5; i < components.length; i++) {
             if (components[i] == '..') {
                 upCount++;
             } else {
                 break;
             }
         }

         if (upCount != 0) {
             var dirComponents = dirname.split('\\');
             if(upCount > 0) {
                dirComponents.pop();
             }
             if(upCount > 1) {
                dirComponents.pop();
             }
             if(upCount > 2) {
                dirComponents.pop();
             }
             if(upCount > 3) {
                dirComponents.pop();
             }
             for(var i = 4; i < upCount; i++) {
                 dirComponents.pop();
             }
             dirComponents = dirComponents.concat(components.slice(upCount+1, components.length-1));
             var res = match_dir === dirComponents.join('\\') && (components[components.length - 1] == mod_name || components[components.length - 1] == alt_mod_name);
             return res;
         }
     }
     return false;
}

function starts_with(a, b) {
    return a.substr(0, b.length) == b;
}
");

            // If we get passed a fully qualified path turn it into a relative path
            switchCode.Append("function relative(from, to) {");
            switchCode.AppendLine(ReferenceCode.PathRelativeBody);
            switchCode.Append("}");

            switchCode.Append(@"if(module[1] == ':') { 
    intellisense.logMessage('making relative ' + __dirname + ' -- ' + module); 
    new_module = relative(__dirname, module); 
    if (new_module != module) {
        module = './' + new_module;
    }
    intellisense.logMessage('now ' + module); 
}");

            foreach (NodejsFileNode nodeFile in _nodeFiles) {
                if (nodeFile.Url.Length > NativeMethods.MAX_PATH) {
                    // .NET can't handle long filename paths, so ignore these modules...
                    continue;
                }

                // for each file we setup if statements which check and see if the current
                // file doing the require() call will match based upon the string being
                // passed in.   
                //
                // __dirname is the directory that the file doing the require call exists 
                // in during intellisense
                //
                // module is the string passed in by the user to require(...)
                //

                switchCode.Append("if(");
                for (var curParent = nodeFile.Parent; curParent != null; curParent = curParent.Parent) {
                    if (curParent != nodeFile.Parent) {
                        switchCode.Append(" || ");
                    }

                    var baseDir = curParent.FullPathToChildren;
                    var trimmedBaseDir = CommonUtils.TrimEndSeparator(baseDir);
                    string name = CommonUtils.CreateFriendlyFilePath(
                            baseDir,
                            nodeFile.Url
                        ).Replace("\\", "/");

                    // For each parent above the file, the file can be accessed with a relative
                    // path, e.g ./my/parent and with or without a .js extension
                    switchCode.AppendFormat(
                        "    (__dirname == '{0}' && (module == './{1}' || module == './{2}')) || relative_match('{0}', __dirname, '{1}', '{2}', module)\r\n",
                        trimmedBaseDir.Replace("\\", "\\\\"),
                        Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name)).Replace('\\', '/'),
                        name
                    );

                    if (String.Equals(Path.GetFileName(trimmedBaseDir), NodejsConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase)) {
                        // when we're in node_modules we're accessible by a plain name as well, and
                        // we're accessible from either our immediate parent directory or children
                        // within the package.
                        var filePath = CommonUtils.CreateFriendlyFilePath(baseDir, nodeFile.Url);
                        string parentBaseDir = Path.GetDirectoryName(CommonUtils.TrimEndSeparator(baseDir));

                        switchCode.AppendFormat(
                            "    || ((__dirname == '{0}' || __dirname == '{1}') && (module == '{2}' || module == '{3}'))\r\n",
                            parentBaseDir.Replace("\\", "\\\\"),
                            trimmedBaseDir.Replace("\\", "\\\\"),
                            Path.GetFileNameWithoutExtension(filePath),
                            Path.GetFileName(filePath)
                        );
                    }

                    List<CommonFolderNode> folderPackageList;
                    if (directoryPackages.TryGetValue(nodeFile, out folderPackageList)) {
                        foreach (var folderPackage in folderPackageList) {
                                // this file is also exposed as the folder
                            if (Path.GetFileName(CommonUtils.TrimEndSeparator(folderPackage.Parent.Url)) == NodejsConstants.NodeModulesFolder) {
                                switchCode.AppendFormat(
                                    "    || (starts_with(__dirname, '{0}') && (module == '{1}'))\r\n",
                                    CommonUtils.TrimEndSeparator(folderPackage.Parent.Parent.FullPathToChildren).Replace("\\", "\\\\"),
                                    Path.GetFileName(CommonUtils.TrimEndSeparator(folderPackage.Url))
                                );
                            } else {
                                switchCode.AppendFormat(
                                    "    || (starts_with(__dirname, '{0}') && (module == './{1}' || module == './{1}/'))\r\n",
                                    CommonUtils.TrimEndSeparator(folderPackage.Parent.FullPathToChildren).Replace("\\", "\\\\"),
                                    Path.GetFileName(CommonUtils.TrimEndSeparator(folderPackage.Url))
                                );
                            }
                        }
                    }

                    if (String.Equals(Path.GetFileName(trimmedBaseDir), NodejsConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase)) {
                        break;
                    }
                }

                switchCode.Append(") {");
                switchCode.AppendLine("intellisense.progress();");
                switchCode.AppendFormat("if (typeof {0}{1} == 'undefined') {{",
                    NodejsConstants.NodejsHiddenUserModuleInstance,
                    nodeFile._fileId
                );

                // Scale back the depth of analysis based upon how many requires a package does
                // The reference file automatically restores max_require_depth back to it's default
                // after this require finishes executing.
                if (nodeFile._requireCount >= 25) {
                    switchCode.AppendLine("max_require_depth -= 2;");
                } else if (nodeFile._requireCount >= 10) {
                    switchCode.AppendLine("max_require_depth -= 1;");
                }

                // if we're analyzing too much code bail (if the module was already analyzed at a lower
                // level we'll have already short-circuited and returned its value)
                switchCode.AppendLine("if (require_depth > max_require_depth) { return undefined; }");

                switchCode.AppendFormat("{0}{1} = {2}{1}();",
                    NodejsConstants.NodejsHiddenUserModuleInstance,
                    nodeFile._fileId,
                    NodejsConstants.NodejsHiddenUserModule
                );
                switchCode.AppendLine("}");
                switchCode.AppendFormat("return " + NodejsConstants.NodejsHiddenUserModuleInstance + "{0};", nodeFile._fileId);
                switchCode.AppendLine("}");
            }

            switchCode.AppendLine("intellisense.logMessage('Intellisense failed to resolve module: ' + module + ' in ' + __filename);");
        }

        /// <summary>
        /// Generates a mapping from file node to folder node.  The file node is the node which
        /// is the entry point for the folder in node_modules.  Typically the file will be index.js
        /// or a file specified in package.json.
        /// </summary>
        private Dictionary<FileNode, List<CommonFolderNode>> GetModuleFolderMapping() {
            List<CommonFolderNode> folderNodes = new List<CommonFolderNode>();
            FindNodesOfType<CommonFolderNode>(folderNodes);
            var directoryPackages = new Dictionary<FileNode, List<CommonFolderNode>>();

            // collect all of the packages in node_modules folders and their associated entry point
            foreach (var folderNode in folderNodes) {
                if (String.Equals(
                    Path.GetFileName(CommonUtils.TrimEndSeparator(folderNode.Url)),
                    NodejsConstants.NodeModulesFolder,
                    StringComparison.OrdinalIgnoreCase)) {

                    for (var curChild = folderNode.FirstChild; curChild != null; curChild = curChild.NextSibling) {
                        CommonFolderNode folderChild = curChild as CommonFolderNode;
                        if (folderChild != null) {
                            var packageJsonChild = curChild.FindImmediateChildByName(NodejsConstants.PackageJsonFile);

                            Dictionary<string, object> packageJson = null;
                            if (packageJsonChild != null && File.Exists(packageJsonChild.Url)) {
                                try {
                                    packageJson = (Dictionary<string, object>)_serializer.DeserializeObject(File.ReadAllText(packageJsonChild.Url));
                                } catch (Exception e) {
                                    // can't read, failed to deserialize json, fallback to index.js if it exists
                                    var outputWindow = Package.GetOutputPane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "General");
                                    if (outputWindow != null) {
                                        outputWindow.OutputStringThreadSafe(String.Format("Failed to parse {0}:\r\n\r\n{1}\r\n\r\n", packageJsonChild.Url, e.Message));
                                    }
                                }
                            }

                            object mainFile;
                            string mainFileStr;
                            if (packageJson != null &&
                                packageJson.TryGetValue(NodejsConstants.PackageJsonMainFileKey, out mainFile) &&
                                (mainFileStr = mainFile as string) != null) {

                                if (mainFileStr.StartsWith("./")) {
                                    mainFileStr = mainFileStr.Substring(2);
                                }
                                mainFileStr = mainFileStr.Replace('/', '\\');
                                var pathToRootFile = Path.Combine(folderChild.FullPathToChildren, mainFileStr);
                                var rootFile = FindNodeByFullPath(pathToRootFile) as FileNode;
                                if (rootFile == null) {
                                    rootFile = FindNodeByFullPath(pathToRootFile + NodejsConstants.FileExtension) as FileNode;
                                    if (rootFile == null) {
                                        rootFile = FindNodeByFullPath(Path.Combine(pathToRootFile, "index.js")) as FileNode;
                                    }
                                }

                                if (rootFile != null) {
                                    AddFolderForFile(directoryPackages, rootFile, folderChild);
                                }
                            }


                            var indexJsChild = curChild.FindImmediateChildByName(NodejsConstants.DefaultPackageMainFile) as FileNode;
                            if (indexJsChild != null && File.Exists(indexJsChild.Url)) {
                                AddFolderForFile(directoryPackages, indexJsChild, folderChild);
                            }
                        }
                    }
                } else {
                    var indexJsChild = folderNode.FindImmediateChildByName(NodejsConstants.DefaultPackageMainFile) as FileNode;
                    if (indexJsChild != null && File.Exists(indexJsChild.Url)) {
                        AddFolderForFile(directoryPackages, indexJsChild, folderNode);
                    }
                }
            }
            return directoryPackages;
        }

        private static void AddFolderForFile(Dictionary<FileNode, List<CommonFolderNode>> directoryPackages, FileNode rootFile, CommonFolderNode folderChild) {
            List<CommonFolderNode> folders;
            if (!directoryPackages.TryGetValue(rootFile, out folders)) {
                directoryPackages[rootFile] = folders = new List<CommonFolderNode>();
            }
            folders.Add(folderChild);
        }

        /// <summary>
        /// Reads our baseline Node.js reference code.  If it doesn't exist for some reason
        /// it'll use an empty skeleton.
        /// </summary>
        private static string ReadNodeRefCode() {
            string nodeFile = NodejsPackage.NodejsReferencePath;

            if (File.Exists(nodeFile)) {
                try {
                    return File.ReadAllText(nodeFile);
                } catch {
                }
            }

            // should never happen, we failed to find our baseline node code
            // or failed to read it.
            return @"
function require(module) {
    switch (module) {
    " + _userSwitchMarker + @"
    }
}
";
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



        protected internal override void ProcessReferences()
        {
            base.ProcessReferences();

            if ( null == ModulesNode )
            {
                ModulesNode = new NodeModulesNode( this );
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
            get { return (OAProject)GetAutomationObject();  }
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
    }
}
