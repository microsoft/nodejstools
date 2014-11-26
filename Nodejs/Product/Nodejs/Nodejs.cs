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
#if !NO_WINDOWS
using System.Windows.Forms;
#endif
using Microsoft.Win32;
#if !NO_WINDOWS
using Microsoft.NodejsTools.Project;
using System.Security;
#endif

namespace Microsoft.NodejsTools {
    public sealed class Nodejs {
        private const string NodejsRegPath = "Software\\Node.js";
        private const string InstallPath = "InstallPath";

        private static string _nodeExePath = null;

        public static string NodeExePath {
            get {
                if (_nodeExePath == null) {
                    _nodeExePath = GetPathToNodeExecutable();
                }
                return _nodeExePath;
            }
        }

        public static string GetPathToNodeExecutable(string executable = "node.exe") {
            // Attempt to find Node.js/NPM in the Registry. (Currrent User)
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            using (var node = baseKey.OpenSubKey(NodejsRegPath)) {
                if (node != null) {
                    string key = (node.GetValue(InstallPath) as string) ?? string.Empty;
                    var execPath = Path.Combine(key, executable);
                    if (File.Exists(execPath)) {
                        return execPath;
                    }
                }
            }

            // Attempt to find Node.js/NPM in the Registry. (Local Machine x64)
            if (Environment.Is64BitOperatingSystem) {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var node64 = baseKey.OpenSubKey(NodejsRegPath)) {
                    if (node64 != null) {
                        string key = (node64.GetValue(InstallPath) as string) ?? string.Empty;
                        var execPath = Path.Combine(key, executable);
                        if (File.Exists(execPath)) {
                            return execPath;
                        }
                    }
                }
            }

            // Attempt to find Node.js/NPM in the Registry. (Local Machine x86)
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var node = baseKey.OpenSubKey(NodejsRegPath)) {
                if (node != null) {
                    string key = (node.GetValue(InstallPath) as string) ?? string.Empty;
                    var execPath = Path.Combine(key, executable);
                    if (File.Exists(execPath)) {
                        return execPath;
                    }
                }
            }

            // If we didn't find node.js in the registry we should look at the user's path.
            foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator)) {
                var execPath = Path.Combine(dir, executable);
                if (File.Exists(execPath)) {
                    return execPath;
                }
            }

            // It wasn't in the users path.  Check Program Files for the nodejs folder.
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", executable);
            if (File.Exists(path)) {
                return path;
            }

            // It wasn't in the users path.  Check Program Files x86 for the nodejs folder.
            var x86path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!String.IsNullOrEmpty(x86path)) {
                path = Path.Combine(x86path, "nodejs", executable);
                if (File.Exists(path)) {
                    return path;
                }
            }

            // we didn't find the path.
            return null;
        }

#if !NO_WINDOWS
        public static void ShowNodejsNotInstalled() {
            MessageBox.Show(
                SR.GetString(SR.NodejsNotInstalled),
                SR.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        /// <summary>
        /// Checks if the given version of Node.js is supported and displays a
        /// message if it isn't.  Returns true if the version is supported.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckNodejsSupported(string path) {
            bool supported = true;
            if (path != null && File.Exists(path) &&
                string.Compare(Path.GetFileName(path), "node.exe", StringComparison.OrdinalIgnoreCase) == 0) {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
                if (info.FileMajorPart == 0) {
                    if (info.FileMinorPart < 10 ||
                        (info.FileMinorPart == 10 && info.FileBuildPart < 20)) {
                        MessageBox.Show(
                            SR.GetString(SR.NodejsNotSupported, info.FileVersion),
                            SR.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        supported = false;
                    }
                }
            }
            return supported;
        }

        public static void ShowNodejsPathNotFound(string path) {
            MessageBox.Show(
                SR.GetString(SR.NodeExeDoesntExist, path),
                SR.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
#endif
    }
}
