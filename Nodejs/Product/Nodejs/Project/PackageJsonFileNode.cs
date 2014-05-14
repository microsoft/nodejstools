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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    class PackageJsonFileNode : CommonNonCodeFileNode {
        private FileSystemWatcher _watcher;

        public PackageJsonFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
            CreateWatcher(Url);
            FileContentsChanged(this, null);
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
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
            }
        }

        private void FileContentsChanged(object sender, FileSystemEventArgs e) {
            string fileContents = null;
            for (int i = 0; i < 10; i++) {
                try {
                    fileContents = File.ReadAllText(Url);
                    break;
                } catch {
                    Thread.Sleep(100);
                }
            }

            if (fileContents != null) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> json;
                try {
                    json = serializer.Deserialize<Dictionary<string, object>>(fileContents);
                } catch {
                    return;
                }
                
                object mainFile;
                if (json.TryGetValue("main", out mainFile) && mainFile is string) {
                    ((NodejsProjectNode)ProjectMgr).Analyzer.Project.AddPackageJson(Url, (string)mainFile);
                }
            }
        }
    }
}
