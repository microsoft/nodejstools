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
#endif

namespace Microsoft.NodejsTools {
    public sealed class Nodejs {
        private static string _nodeExePath = null;

        public static string NodeExePath {
            get {
                if (_nodeExePath == null) {
                    //Fall back to a well known location if lookup fails
                    string installPath = null;
                    try {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Node.js")) {
                            if (key != null) {
                                string keyValue = (string)key.GetValue("InstallPath", installPath);
                                installPath = String.IsNullOrEmpty(keyValue) ? installPath : keyValue;
                            }
                        }
                    } catch (Exception) {
                    }

                    if (installPath == null) {
                        foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator)) {
                            if (File.Exists(Path.Combine(dir, "node.exe"))) {
                                installPath = dir;
                                break;
                            }
                        }

                    }

                    if (installPath == null) {
                        string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs");
                        if (Directory.Exists(tempPath)) {
                            installPath = tempPath;
                        }
                    }

                    if (installPath == null) {
                        var x86path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        if (!String.IsNullOrEmpty(x86path)) {
                            string tempPath = Path.Combine(x86path, "nodejs");
                            if (Directory.Exists(tempPath)) {
                                installPath = tempPath;
                            }
                        }
                    }

                    if (installPath != null) {
                        string path = Path.Combine(installPath, "node.exe");
                        if (File.Exists(path)) {
                            _nodeExePath = Path.Combine(installPath, "node.exe");
                        }
                    }
                }

                return _nodeExePath;
            }
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
