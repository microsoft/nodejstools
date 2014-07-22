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

namespace Microsoft.NodejsTools.Npm {
    public static class NpmHelpers {

        public static string GetPathToNpm(
            bool useFallbackIfNpmNotFound = true) 
        {
            string pathToNpm = null;
            if (useFallbackIfNpmNotFound) {
                string match = null;

                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Node.js")) {
                    if (key != null) {
                        match = key.GetValue("InstallPath") as string;
                        if (Directory.Exists(match)) {
                            match = Path.Combine(match, "npm.cmd");
                            if (!File.Exists(match)) {
                                match = null;
                            }
                        }
                    }
                }

                if (null == match) {
                    foreach (var potential in Environment.GetEnvironmentVariable("path").Split(Path.PathSeparator)) {
                        var path = Path.Combine(potential, "npm.cmd");
                        if (File.Exists(path)) {
                            if (null == match ||
                                path.Contains(
                                    string.Format(
                                        "{0}nodejs{1}",
                                        Path.DirectorySeparatorChar,
                                        Path.DirectorySeparatorChar))) {
                                match = path;
                            }
                        }
                    }
                }

                if (null != match) {
                    pathToNpm = match;
                }
            }

            if (null == pathToNpm || !File.Exists(pathToNpm)) {
                throw new NpmNotFoundException(
                    string.Format(
                        "Cannot find npm.cmd at '{0}' nor on your system PATH. Ensure Node.js is installed.",
                        pathToNpm ?? "(null)"
                    )
                );
            }
            return pathToNpm;
        }
    }
}
