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

using System.IO;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;

namespace Microsoft.NodejsTools.Project {
    class NodejsFileNode : CommonFileNode {
        private FileSystemWatcher _watcher;
        public string _currentText;

        public NodejsFileNode(NodejsProjectNode root, MsBuildProjectElement e)
            : base(root, e) {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(Url), Path.GetFileName(Url));
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += FileContentsChanged;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _currentText = File.ReadAllText(Url);
            lock (root._nodeFiles) {
                root._nodeFiles.Add(this);
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
            ((NodejsProjectNode)this.ProjectMgr).UpdateReferenceFile();
        }

        public override void Remove(bool removeFromStorage) {
            base.Remove(removeFromStorage);
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        public override int Close() {
            int res;
            if (ErrorHandler.Succeeded(res = base.Close())) {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
            lock (((NodejsProjectNode)ProjectMgr)._nodeFiles) {
                ((NodejsProjectNode)ProjectMgr)._nodeFiles.Remove(this);
            }
            return res;
        }
    }
}
