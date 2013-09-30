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
using System.IO;
using Microsoft.Win32;

namespace Microsoft.NodejsTools {
    public sealed class Nodejs {
        private static string _nodeExePath = null;

        public static string NodeExePath {
            get {
                if (_nodeExePath != null) {
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
    }
}
