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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Npm {
    public static class NpmHelpers {

        internal static async Task<IEnumerable<string>> ExecuteNpmCommandAsync(
            Redirector redirector, 
            string pathToNpm,
            string executionDirectory,
            string[] arguments,
            ManualResetEvent cancellationResetEvent) {

            IEnumerable<string> standardOutputLines = null;

            using (var process = ProcessOutput.Run(
                pathToNpm,
                arguments,
                executionDirectory,
                null,
                false,
                redirector,
                quoteArgs: false,
                outputEncoding: Encoding.UTF8 // npm uses UTF-8 regardless of locale if its output is redirected
                )) {
                var whnd = process.WaitHandle;
                if (whnd == null) {
                    // Process failed to start, and any exception message has
                    // already been sent through the redirector
                    if (redirector != null) {
                        redirector.WriteErrorLine(Resources.ErrCannotStartNpm);
                    }
                } else {
                    var handles = cancellationResetEvent != null ? new[] { whnd, cancellationResetEvent } : new [] { whnd };
                    var i = await Task.Run(() => WaitHandle.WaitAny(handles));
                    if (i == 0) {
                        Debug.Assert(process.ExitCode.HasValue, "npm process has not really exited");
                        process.Wait();

                        if (process.StandardOutputLines != null) {
                            standardOutputLines = process.StandardOutputLines.ToList();                            
                        }
                        if (redirector != null) {
                            redirector.WriteLine(string.Format(
                                "\r\n===={0}====\r\n\r\n",
                                string.Format(Resources.NpmCommandCompletedWithExitCode, process.ExitCode ?? -1)
                                ));
                        }
                    } else {
                        process.Kill();
                        if (redirector != null) {
                            redirector.WriteErrorLine(string.Format(
                            "\r\n===={0}====\r\n\r\n",
                            Resources.NpmCommandCancelled));
                        }
                        
                        if (cancellationResetEvent != null) {
                            cancellationResetEvent.Reset();
                        }
                        throw new OperationCanceledException();
                    }
                }
            }
            return standardOutputLines;
        }


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
