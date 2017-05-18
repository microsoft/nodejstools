// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IReplCommand))]
    internal class NpmReplCommand : IReplCommand
    {
        #region IReplCommand Members

        public async Task<ExecutionResult> Execute(IReplWindow window, string arguments)
        {
            var projectPath = string.Empty;
            var npmArguments = arguments.Trim(' ', '\t');

            // Parse project name/directory in square brackets
            if (npmArguments.StartsWith("[", StringComparison.Ordinal))
            {
                var match = Regex.Match(npmArguments, @"(?:[[]\s*\""?\s*)(.*?)(?:\s*\""?\s*[]]\s*)");
                projectPath = match.Groups[1].Value;
                npmArguments = npmArguments.Substring(match.Length);
            }

            // Include spaces on either side of npm arguments so that we can more simply detect arguments
            // at beginning and end of string (e.g. '--global')
            npmArguments = string.Format(CultureInfo.InvariantCulture, " {0} ", npmArguments);

            // Prevent running `npm init` without the `-y` flag since it will freeze the repl window,
            // waiting for user input that will never come.
            if (npmArguments.Contains(" init ") && !(npmArguments.Contains(" -y ") || npmArguments.Contains(" --yes ")))
            {
                window.WriteError(Resources.ReplWindowNpmInitNoYesFlagWarning);
                return ExecutionResult.Failure;
            }

            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            var loadedProjects = solution.EnumerateLoadedProjects(onlyNodeProjects: false);

            var projectNameToDirectoryDictionary = new Dictionary<string, Tuple<string, IVsHierarchy>>(StringComparer.OrdinalIgnoreCase);
            foreach (var project in loadedProjects)
            {
                var hierarchy = (IVsHierarchy)project;
                object extObject;

                var projectResult = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject);
                if (!ErrorHandler.Succeeded(projectResult))
                {
                    continue;
                }

                var dteProject = extObject as EnvDTE.Project;
                if (dteProject == null)
                {
                    continue;
                }

                var projectName = dteProject.Name;
                if (string.IsNullOrEmpty(projectName))
                {
                    continue;
                }

                // Try checking the `ProjectHome` property first
                var properties = dteProject.Properties;
                if (dteProject.Properties != null)
                {
                    EnvDTE.Property projectHome = null;
                    try
                    {
                        projectHome = properties.Item("ProjectHome");
                    }
                    catch (ArgumentException)
                    {
                        // noop
                    }

                    if (projectHome != null)
                    {
                        var projectHomeDirectory = projectHome.Value as string;
                        if (!string.IsNullOrEmpty(projectHomeDirectory))
                        {
                            projectNameToDirectoryDictionary.Add(projectName, Tuple.Create(projectHomeDirectory, hierarchy));
                            continue;
                        }
                    }
                }

                // Otherwise, fall back to using fullname
                var projectDirectory = string.IsNullOrEmpty(dteProject.FullName) ? null : Path.GetDirectoryName(dteProject.FullName);
                if (!string.IsNullOrEmpty(projectDirectory))
                {
                    projectNameToDirectoryDictionary.Add(projectName, Tuple.Create(projectDirectory, hierarchy));
                }
            }

            Tuple<string, IVsHierarchy> projectInfo;
            if (string.IsNullOrEmpty(projectPath) && projectNameToDirectoryDictionary.Count == 1)
            {
                projectInfo = projectNameToDirectoryDictionary.Values.First();
            }
            else
            {
                projectNameToDirectoryDictionary.TryGetValue(projectPath, out projectInfo);
            }

            NodejsProjectNode nodejsProject = null;
            if (projectInfo != null)
            {
                projectPath = projectInfo.Item1;
                if (projectInfo.Item2 != null)
                {
                    nodejsProject = projectInfo.Item2.GetProject().GetNodejsProject();
                }
            }

            var isGlobalCommand = false;
            if (string.IsNullOrWhiteSpace(npmArguments) ||
                npmArguments.Contains(" -g ") || npmArguments.Contains(" --global "))
            {
                projectPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                isGlobalCommand = true;
            }

            // In case someone copies filename
            var projectDirectoryPath = File.Exists(projectPath) ? Path.GetDirectoryName(projectPath) : projectPath;

            if (!isGlobalCommand && !Directory.Exists(projectDirectoryPath))
            {
                window.WriteError("Please specify a valid Node.js project or project directory. If your solution contains multiple projects, specify a target project using .npm [ProjectName or ProjectDir] <npm arguments> For example: .npm [MyApp] list");
                return ExecutionResult.Failure;
            }

            string npmPath;
            try
            {
                npmPath = NpmHelpers.GetPathToNpm(
                    nodejsProject != null ?
                        Nodejs.GetAbsoluteNodeExePath(
                            nodejsProject.ProjectHome,
                            nodejsProject.GetProjectProperty(NodeProjectProperty.NodeExePath))
                        : null);
            }
            catch (NpmNotFoundException)
            {
                Nodejs.ShowNodejsNotInstalled();
                return ExecutionResult.Failure;
            }

            var npmReplRedirector = new NpmReplRedirector(window);
            await ExecuteNpmCommandAsync(
                npmReplRedirector,
                npmPath,
                projectDirectoryPath,
                new[] { npmArguments },
                null);

            if (npmReplRedirector.HasErrors)
            {
                window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.NpmReplCommandCompletedWithErrors, arguments));
            }
            else
            {
                window.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.NpmSuccessfullyCompleted, arguments));
            }

            if (nodejsProject != null)
            {
                await nodejsProject.CheckForLongPaths(npmArguments);
            }

            return ExecutionResult.Success;
        }

        public string Description => "Executes npm command. If solution contains multiple projects, specify target project using .npm [ProjectName] <npm arguments>";
        public string Command => "npm";
        public object ButtonContent => null;
        // TODO: This is duplicated from Npm project
        // We should consider using InternalsVisibleTo to avoid code duplication
        internal static async Task<IEnumerable<string>> ExecuteNpmCommandAsync(
            Redirector redirector,
            string pathToNpm,
            string executionDirectory,
            string[] arguments,
            ManualResetEvent cancellationResetEvent)
        {
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
                ))
            {
                var whnd = process.WaitHandle;
                if (whnd == null)
                {
                    // Process failed to start, and any exception message has
                    // already been sent through the redirector
                    if (redirector != null)
                    {
                        redirector.WriteErrorLine("Error - cannot start npm");
                    }
                }
                else
                {
                    var handles = cancellationResetEvent != null ? new[] { whnd, cancellationResetEvent } : new[] { whnd };
                    var i = await Task.Run(() => WaitHandle.WaitAny(handles));
                    if (i == 0)
                    {
                        Debug.Assert(process.ExitCode.HasValue, "npm process has not really exited");
                        process.Wait();
                        if (process.StandardOutputLines != null)
                        {
                            standardOutputLines = process.StandardOutputLines.ToList();
                        }
                    }
                    else
                    {
                        process.Kill();
                        if (redirector != null)
                        {
                            redirector.WriteErrorLine(string.Format(CultureInfo.CurrentCulture,
                            "\r\n===={0}====\r\n\r\n",
                            "npm command cancelled"));
                        }

                        if (cancellationResetEvent != null)
                        {
                            cancellationResetEvent.Reset();
                        }

                        throw new OperationCanceledException();
                    }
                }
            }
            return standardOutputLines;
        }

        #endregion

        internal class NpmReplRedirector : Redirector
        {
            internal const string ErrorAnsiColor = "\x1b[31;1m";
            internal const string WarnAnsiColor = "\x1b[33;22m";
            internal const string NormalAnsiColor = "\x1b[39;49m";

            private const string ErrorText = "npm ERR!";
            private const string WarningText = "npm WARN";

            private IReplWindow _window;

            public NpmReplRedirector(IReplWindow window)
            {
                this._window = window;
                this.HasErrors = false;
            }
            public bool HasErrors { get; set; }

            public override void WriteLine(string decodedString)
            {
                var substring = string.Empty;
                var outputString = string.Empty;

                if (decodedString.StartsWith(ErrorText, StringComparison.Ordinal))
                {
                    outputString += ErrorAnsiColor + decodedString.Substring(0, ErrorText.Length);
                    substring = decodedString.Length > ErrorText.Length ? decodedString.Substring(ErrorText.Length) : string.Empty;
                    this.HasErrors = true;
                }
                else if (decodedString.StartsWith(WarningText, StringComparison.Ordinal))
                {
                    outputString += WarnAnsiColor + decodedString.Substring(0, WarningText.Length);
                    substring = decodedString.Length > WarningText.Length ? decodedString.Substring(WarningText.Length) : string.Empty;
                }
                else
                {
                    substring = decodedString;
                }

                outputString += NormalAnsiColor + substring;

                this._window.WriteLine(outputString);
                Debug.WriteLine(decodedString, "REPL npm");
            }

            public override void WriteErrorLine(string line)
            {
                this._window.WriteError(line);
            }
        }
    }
}
