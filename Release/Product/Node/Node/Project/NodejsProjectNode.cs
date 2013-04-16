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
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;

namespace Microsoft.NodejsTools.Project {
    class NodejsProjectNode : CommonProjectNode, VsWebSite.VSWebSite {
        private static string _nodeRefCode = ReadNodeRefCode();
        internal string _referenceFilename = GetReferenceFilePath();
        const string _userSwitchMarker = "// **NTVS** INSERT USER MODULE SWITCH HERE **NTVS**";
        internal readonly List<NodejsFileNode> _nodeFiles = new List<NodejsFileNode>();
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public NodejsProjectNode(NodejsProjectPackage package)
            : base(package, Utilities.GetImageList(typeof(NodejsProjectNode).Assembly.GetManifestResourceStream("Microsoft.NodejsTools.NodeImageList.bmp"))) {

            Type projectNodePropsType = typeof(NodejsProjectNodeProperties);
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
        }

        public override int InitializeForOuter(string filename, string location, string name, uint flags, ref Guid iid, out IntPtr projectPointer, out int canceled) {
            int res = base.InitializeForOuter(filename, location, name, flags, ref iid, out projectPointer, out canceled);
            if (ErrorHandler.Succeeded(res)) {
                UpdateReferenceFile();
            }
            return res;
        }

        public override void Close() {
            base.Close();
            if (File.Exists(_referenceFilename)) {
                File.Delete(_referenceFilename);
            }
        }

        public override string[] CodeFileExtensions {
            get {
                return new[] { NodeConstants.FileExtension };
            }
        }

        private static string GetReferenceFilePath() {
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

        public override CommonFileNode CreateCodeFileNode(MsBuildProjectElement item) {
            return new NodejsFileNode(this, item);
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
            return NodeConstants.ProjectFileFilter;
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

        /// <summary>
        /// Updates our per-project reference .js file.  This file gets our baseline node reference file
        /// merged with all of the user's code as well.  This allows the user to do require('mymodule') and
        /// get the correct intellisense back.
        /// </summary>
        internal void UpdateReferenceFile() {
            StringBuilder switchCode = new StringBuilder();
            StringBuilder moduleCode = new StringBuilder();

            lock (_nodeFiles) {
                UpdateReferenceFile(this, switchCode);

                try {
                    File.WriteAllText(
                        _referenceFilename,
                        _nodeRefCode.Replace(_userSwitchMarker, switchCode.ToString())
                    );
                } catch (IOException) {
                }
            }
        }

        /// <summary>
        /// Walks the project and generates the reference code for each .js file present.
        /// </summary>
        private void UpdateReferenceFile(HierarchyNode node, StringBuilder switchCode) {
            // collect all of the node_modules folders
            Dictionary<FileNode, CommonFolderNode> directoryPackages = GetModuleFolderMapping();

            int moduleId = 0;
            switchCode.Append("default:");
            switchCode.Append(@"
function relative_match(match_dir, dirname, mod_name, alt_mod_name, ref_path) {
     if(ref_path.substr(0, 3) == '../') {
        // fall into ./ case below
        ref_path = './' + ref_path;
     }
     if(ref_path.substr(0, 2) == './') {
         var components = ref_path.split('/');
         var upCount = 0;
         for(var i = 1; i < components.length; i++) {
             if (components[i] == '..') {
                 upCount++;
             } else {
                 break;
             }
         }
         if (upCount != 0) {
             var dirComponents = dirname.split('\\');
             for(var i = 0; i < upCount; i++) {
                 dirComponents.pop();
             }
             for (var i = upCount + 1; i < (components.length - 1); i++) {
                 dirComponents.push(components[i]);
             }
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
            foreach (NodejsFileNode nodeFile in _nodeFiles) {
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

                    if (String.Equals(Path.GetFileName(trimmedBaseDir), NodeConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase)) {
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

                    CommonFolderNode folderPackage;
                    if (directoryPackages.TryGetValue(nodeFile, out folderPackage)) {
                        // this file is also exposed as the folder
                        switchCode.AppendFormat(
                            "    || (starts_with(__dirname, '{0}') && (module == '{1}'))\r\n",
                            CommonUtils.TrimEndSeparator(folderPackage.Parent.Parent.FullPathToChildren).Replace("\\", "\\\\"),
                            Path.GetFileName(CommonUtils.TrimEndSeparator(folderPackage.Url))
                        );
                    }
                }

                switchCode.Append(") {");
                switchCode.AppendFormat("if(global[{0}] === undefined) {{ ", moduleId);
                switchCode.Append(
                    NodejsProjectionBuffer.GetNodeFunctionWrapperHeader(
                        "module_name" + moduleId,
                        nodeFile.Url
                    )
                );

                // publish it at the start for recursive modules, may change later if user 
                // does module.exports = .... in which case we will republish at the end.
                switchCode.AppendFormat("global[{0}] = module.exports;", moduleId); 

                switchCode.Append(nodeFile._currentText);
                switchCode.Append(NodejsProjectionBuffer.TrailingText);
                switchCode.AppendFormat("global[{0}] = module_name{0}();", moduleId);
                switchCode.AppendLine("}");
                switchCode.AppendFormat("return global[{0}];", moduleId);
                switchCode.AppendLine("}");
                moduleId++;
            }
            switchCode.Append("break;");
        }

        /// <summary>
        /// Generates a mapping from file node to folder node.  The file node is the node which
        /// is the entry point for the folder in node_modules.  Typically the file will be index.js
        /// or a file specified in package.json.
        /// </summary>
        private Dictionary<FileNode, CommonFolderNode> GetModuleFolderMapping() {
            List<CommonFolderNode> folderNodes = new List<CommonFolderNode>();
            FindNodesOfType<CommonFolderNode>(folderNodes);
            Dictionary<FileNode, CommonFolderNode> directoryPackages = new Dictionary<FileNode, CommonFolderNode>();

            // collect all of the packages in node_modules folders and their associated entry point
            foreach (var folderNode in folderNodes) {
                if (String.Equals(
                    Path.GetFileName(CommonUtils.TrimEndSeparator(folderNode.Url)),
                    NodeConstants.NodeModulesFolder,
                    StringComparison.OrdinalIgnoreCase)) {

                    for (var curChild = folderNode.FirstChild; curChild != null; curChild = curChild.NextSibling) {
                        CommonFolderNode folderChild = curChild as CommonFolderNode;
                        if (folderChild != null) {
                            var packageJsonChild = curChild.FindImmediateChildByName(NodeConstants.PackageJsonFile);

                            Dictionary<string, object> packageJson = null;
                            if (packageJsonChild != null && File.Exists(packageJsonChild.Url)) {
                                try {
                                    packageJson = (Dictionary<string, object>)_serializer.DeserializeObject(File.ReadAllText(packageJsonChild.Url));
                                } catch(Exception e) {
                                    // can't read, failed to deserialize json, fallback to index.js if it exists
                                    var outputWindow = Package.GetOutputPane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "General");
                                    if (outputWindow != null) {
                                        outputWindow.OutputStringThreadSafe(String.Format("Failed to parse {0}:\r\n\r\n{1}", packageJsonChild.Url, e.Message));
                                    }
                                }
                            }

                            object mainFile;
                            string mainFileStr;
                            if (packageJson != null &&
                                packageJson.TryGetValue(NodeConstants.PackageJsonMainFileKey, out mainFile) &&
                                (mainFileStr = mainFile as string) != null) {

                                if (mainFileStr.StartsWith("./")) {
                                    mainFileStr = mainFileStr.Substring(2);
                                }
                                mainFileStr = mainFileStr.Replace('/', '\\');

                                var rootFile = curChild.FindImmediateChildByName(mainFileStr) as FileNode;
                                if (rootFile == null && Path.GetExtension(mainFileStr) == "") {
                                    rootFile = curChild.FindImmediateChildByName(mainFileStr + ".js") as FileNode;
                                }
                                if (rootFile != null) {
                                    directoryPackages[rootFile] = folderChild;
                                }
                            }


                            var indexJsChild = curChild.FindImmediateChildByName(NodeConstants.DefaultPackageMainFile) as FileNode;
                            if (indexJsChild != null && File.Exists(indexJsChild.Url)) {
                                directoryPackages[indexJsChild] = folderChild;
                            }
                        }
                    }
                }
            }
            return directoryPackages;
        }

        /// <summary>
        /// Reads our baseline Node.js reference code.  If it doesn't exist for some reason
        /// it'll use an empty skeleton.
        /// </summary>
        private static string ReadNodeRefCode() {
            string nodeFile = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "nodejsref.js");

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
