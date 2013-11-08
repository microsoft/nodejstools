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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    class NodejsFileNode : CommonFileNode {
        private FileSystemWatcher _watcher;
        private string _currentText;
        internal int _requireCount;
        internal readonly string _tempFilePath, _asyncFilePath;
        internal readonly ReferenceGroup _refGroup;
        internal int _fileId;
        private static Regex _requireRegexSingleQuote = new Regex("require\\(('.*')\\)", RegexOptions.Compiled);
        private static Regex _requireRegexDoubleQuote = new Regex("require\\((\".*\")\\)", RegexOptions.Compiled);

        public NodejsFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
            CreateWatcher(Url);
            _tempFilePath = NodejsProjectNode.GetReferenceFilePath();
            _asyncFilePath = NodejsProjectNode.GetReferenceFilePath();
            _currentText = "";
            _refGroup = root._refGroupDispenser.AddFile(this);
            _fileId = root._currentFileCounter++;
            if (File.Exists(Url)) { // avoid the exception if we can
                try {
                    _currentText = File.ReadAllText(Url);
                    GenerateReferenceFile();
                } catch {
                }
            }

            root._nodeFiles.Add(this);
        }

        internal string MangledModuleFunctionName {
            get {
                return NodejsConstants.NodejsHiddenUserModule + _fileId;
            }
        }

        private void GenerateReferenceFile() {
            _refGroup.GenerateReferenceFile();
                
            StringBuilder code = new StringBuilder();
            code.AppendLine(NodejsProjectionBuffer.GetFileNameAssignment(Url));
            code.AppendLine(NodejsProjectionBuffer.GetDirectoryNameAssignment(Url));

            int requireCount = 0;
            foreach (Match match in _requireRegexSingleQuote.Matches(_currentText)) {
                code.AppendFormat("require({0});", match.Groups[1]);
                code.AppendLine();
                requireCount++;
            }
            foreach (Match match in _requireRegexDoubleQuote.Matches(_currentText)) {
                code.AppendFormat("require({0});", match.Groups[1]);
                code.AppendLine();
                requireCount++;
            }
            _requireCount = requireCount;
            code.AppendLine();

            NodejsProjectNode.WriteReferenceFile(_tempFilePath, code.ToString());
            NodejsProjectNode.WriteReferenceFile(_asyncFilePath, "_$asyncRequests.add({ src: '" + _tempFilePath.Replace("\\", "\\\\") + "'});\r\n");
        }

        internal string GenerateReferenceCode() {
            StringBuilder code = new StringBuilder();

            code.Append(
                NodejsProjectionBuffer.GetNodeFunctionWrapperHeader(
                    MangledModuleFunctionName,
                    Url
                )
            );
            // publish it at the start for recursive modules, may change later if user 
            // does module.exports = .... in which case we will republish at the end.
            code.AppendFormat(NodejsConstants.NodejsHiddenUserModuleInstance + "{0} = module.exports;", _fileId);
            code.AppendLine();

            code.Append(_currentText);
            code.AppendLine();
            code.AppendFormat(NodejsConstants.NodejsHiddenUserModuleInstance + "{0} = module.exports;", _fileId);
            code.Append(NodejsProjectionBuffer.TrailingText);
            code.AppendLine();
            code.AppendLine();
            return code.ToString();
        }

        internal override int ExecCommandOnNode(Guid guidCmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            Debug.Assert(this.ProjectMgr != null, "The Dynamic FileNode has no project manager");

            Utilities.CheckNotNull(this.ProjectMgr);
            if (guidCmdGroup == GuidList.guidNodeCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidSetAsNodejsStartupFile:
                        // Set the StartupFile project property to the Url of this node
                        ProjectMgr.SetProjectProperty(
                            CommonConstants.StartupFile,
                            CommonUtils.GetRelativeFilePath(this.ProjectMgr.ProjectHome, Url)
                        );
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommandOnNode(guidCmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        internal override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (guidCmdGroup == GuidList.guidNodeCmdSet) {
                if (this.ProjectMgr.IsCodeFile(this.Url)) {
                    switch (cmd) {
                        case PkgCmdId.cmdidSetAsNodejsStartupFile:
                            //We enable "Set as StartUp File" command only on current language code files, 
                            //the file is in project home dir and if the file is not the startup file already.
                            string startupFile = ((CommonProjectNode)ProjectMgr).GetStartupFile();
                            if (!CommonUtils.IsSamePath(startupFile, this.Url) &&
                                !IsNonMemberItem) {
                                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            }
                            return VSConstants.S_OK;
                    }
                }
            }
            return base.QueryStatusOnNode(guidCmdGroup, cmd, pCmdText, ref result);
        }

        private void CloseWatcher() {
            if (_watcher == null) {
                ProjectMgr.UnregisterFileChangeNotification(this);
            } else {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void CreateWatcher(string filename) {
            if (CommonUtils.IsSubpathOf(ProjectMgr.ProjectHome, filename)) {
                // we want to subscribe to the project's file system watcher so users
                // can continue to rename the directory which contains this file.
                ProjectMgr.RegisterFileChangeNotification(this, FileContentsChanged);
            } else {
                // this is a link file which lives outside of our project directory,
                // we'll need to watch the file directly, which means we're going to
                // prevent it's parent directory from being renamed.
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename));
                _watcher.EnableRaisingEvents = true;
                _watcher.Changed += FileContentsChanged;
                _watcher.Renamed += FileContentsChanged;
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
            }
        }

        internal override void RenameInStorage(string oldName, string newName) {
            CloseWatcher();

            base.RenameInStorage(oldName, newName);

            CreateWatcher(newName);
        }

        public override int SetEditLabel(string label) {
            var res = base.SetEditLabel(label);

            ((NodejsProjectNode)this.ProjectMgr).UpdateReferenceFile();

            return res;
        }

        public new NodejsProjectNode ProjectMgr {
            get {
                return (NodejsProjectNode)base.ProjectMgr;
            }
        }

        private void FileContentsChanged(object sender, FileSystemEventArgs e) {
            for (int i = 0; i < 10; i++) {
                try {
                    _currentText = File.ReadAllText(Url);
                    break;
                } catch {
                    System.Threading.Thread.Sleep(250);
                }
            }
            _fileId = ProjectMgr._currentFileCounter++;
            GenerateReferenceFile();

            // Uncomment this line when working on the generated reference file.  It'll enable updating
            // it via a simple file save.  During normal development the reference file only needs to be
            // updated when the files in the project and/or on disk change.
            //ProjectMgr.UpdateReferenceFile(this);
        }

        public override void Remove(bool removeFromStorage) {
            base.Remove(removeFromStorage);
            CloseWatcher();
        }

        public override void Close() {
            base.Close();

            CloseWatcher();

            ((NodejsProjectNode)ProjectMgr)._refGroupDispenser.RemoveFile(this);
            ((NodejsProjectNode)ProjectMgr)._nodeFiles.Remove(this);

            try {
                File.Delete(_tempFilePath);
            } catch (IOException) {
            } catch (UnauthorizedAccessException) {
            }
        }
    }
}
