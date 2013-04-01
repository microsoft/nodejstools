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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
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

        public override int Close() {
            int res = base.Close();
            if (ErrorHandler.Succeeded(res)) {
                if (File.Exists(_referenceFilename)) {
                    File.Delete(_referenceFilename);
                }
            }
            return res;
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

        protected internal override FolderNode CreateFolderNode(string path, ProjectElement element) {
            return new CommonFolderNode(this, path, element);
        }

        public override CommonFileNode CreateCodeFileNode(MsBuildProjectElement item) {
            return new NodejsFileNode(this, item);
        }

        public override string GetProjectName() {
            return "NodeProject";
        }

        public override Type GetProjectFactoryType() {
            return typeof(NodeProjectFactory);
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
            foreach (var nodeFile in _nodeFiles) {
                string name = CommonUtils.CreateFriendlyFilePath(
                        ProjectHome,
                        nodeFile.Url
                    ).Replace("\\", "/");

                switchCode.AppendFormat(@"case ""./{0}"": 
function {1}() {{
var exports = {{}};
{2}

return exports;
}}

return {1}();
", 
                    name, 
                    FilenameToModuleName(name),
                    nodeFile._currentText);
            }
        }

        /// <summary>
        /// Gets a valid identifier name for the module code based upon the filename.
        /// 
        /// This makes sure we're not generating invalid JavaScript.  We use names based upon
        /// what the user gave us, but the name isn't exposed so we could use anything.
        /// </summary>
        private static string FilenameToModuleName(string filename) {
            if (filename.Length == 0 || (!Char.IsLetter(filename[0]) && filename[0] != '_')) {
                filename = "dummy" + filename;
            }
            for (int i = 0; i < filename.Length; i++) {
                if (!Char.IsLetterOrDigit(filename[i]) && filename[i] != '_') {
                    return filename.Substring(0, i);
                }
            }
            return filename;
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
