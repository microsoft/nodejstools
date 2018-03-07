// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.TypeScript
{
    public static class TypeScriptCompile
    {
        private static string CompilerExe = "tsc.exe";

        public static async Task<bool> CompileFileAsync(string filePath, OutputPaneWrapper outputPane, string workingDir = null)
        {
            if (!TypeScriptHelpers.IsTypeScriptFile(filePath) || !Path.IsPathRooted(filePath))
            {
                throw new ArgumentException($"{nameof(filePath)} has to be a rooted '.ts' or '.tsx' file.");
            }

            return await CompileAsync($"\"{filePath}\"", workingDir ?? Path.GetDirectoryName(filePath), new CompileRedirector(outputPane));
        }

        public static async Task<bool> CompileProjectAsync(string filePath, OutputPaneWrapper outputPane)
        {
            if (!TsConfigJsonFactory.IsTsConfigJsonFile(filePath) || !Path.IsPathRooted(filePath))
            {
                throw new ArgumentException($"{nameof(filePath)} has to be a rooted 'tsconfig.json' file.");
            }

            return await CompileAsync($"-p \"{filePath}\"", workingDir: Path.GetDirectoryName(filePath), redirector: new CompileRedirector(outputPane));
        }

        private static async Task<bool> CompileAsync(string arguments, string workingDir, Redirector redirector)
        {
            Debug.Assert(!string.IsNullOrEmpty(arguments), $"{nameof(arguments)} should not be empty.");

            var pathToTsc = Path.Combine(TypeScriptCompilerLocator.GetDefaultVersion(), CompilerExe);

            redirector?.WriteLine($"=== {Resources.TscBuildStarted}: {pathToTsc} {arguments} ===");

            using (var process = ProcessOutput.Run(
                pathToTsc,
                new[] { arguments },
                workingDir,
                env: null,
                visible: false,
                redirector: redirector,
                quoteArgs: false,
                outputEncoding: Encoding.UTF8))
            {
                var whnd = process.WaitHandle;
                if (whnd == null)
                {
                    // Process failed to start, and any exception message has
                    // already been sent through the redirector
                    redirector.WriteErrorLine(string.Format(Resources.TscBuildError, pathToTsc));
                    return false;
                }
                else
                {
                    var finished = await Task.Run(() => whnd.WaitOne());
                    if (finished)
                    {
                        Debug.Assert(process.ExitCode.HasValue, "tsc.exe process has not really exited");
                        // there seems to be a case when we're signalled as completed, but the
                        // process hasn't actually exited
                        process.Wait();

                        redirector.WriteErrorLine($"==== {Resources.TscBuildCompleted} ====");

                        return process.ExitCode == 0;
                    }
                    else
                    {
                        process.Kill();
                        redirector.WriteErrorLine($"==== {Resources.TscBuildCancelled} ====");

                        return false;
                    }
                }
            }
        }

        private sealed class CompileRedirector : Redirector
        {
            private readonly OutputPaneWrapper outputPane;

            public CompileRedirector(OutputPaneWrapper outputPane)
            {
                this.outputPane = outputPane;
            }

            public override void WriteErrorLine(string line)
            {
                this.outputPane.WriteLine(line, OutputWindowTarget.Tsc);
            }

            public override void WriteLine(string line)
            {
                this.outputPane.WriteLine(line, OutputWindowTarget.Tsc);
            }
        }
    }
}
