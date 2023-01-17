// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.NpmUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudioTools.Project;
using EnvDTE;
using Microsoft.NodejsTools.Extras;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IInteractiveWindowCommand))]
    [ContentType(InteractiveWindowContentType.ContentType)]
    internal class NpmReplCommand : InteractiveWindowCommand
    {
        public override async Task<ExecutionResult> Execute(IInteractiveWindow window, string arguments)
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

            var projectNameToDirectoryDictionary = new Dictionary<string, (string, IVsHierarchy)>(StringComparer.OrdinalIgnoreCase);
            foreach (var project in loadedProjects)
            {
                var hierarchy = (IVsHierarchy)project;

                var projectResult = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out var extObject);
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
                            projectNameToDirectoryDictionary.Add(projectName, (projectHomeDirectory, hierarchy));
                            continue;
                        }
                    }
                }

                // Otherwise, fall back to using fullname
                var projectDirectory = string.IsNullOrEmpty(dteProject.FullName) ? null : Path.GetDirectoryName(dteProject.FullName);
                if (!string.IsNullOrEmpty(projectDirectory))
                {
                    projectNameToDirectoryDictionary.Add(projectName, (projectDirectory, hierarchy));
                }
            }

            (string ProjectPath, IVsHierarchy Hierarchy) projectInfo;
            if (string.IsNullOrEmpty(projectPath) && projectNameToDirectoryDictionary.Count == 1)
            {
                projectInfo = projectNameToDirectoryDictionary.Values.First();
            }
            else
            {
                projectNameToDirectoryDictionary.TryGetValue(projectPath, out projectInfo);
            }

            Project nodejsProject = null;
            projectPath = projectInfo.ProjectPath;
            if (projectInfo.Hierarchy != null)
            {
                nodejsProject = projectInfo.Hierarchy.GetProject();
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
                window.WriteError(Resources.NpmSpecifyValidProject);
                return ExecutionResult.Failure;
            }

            string npmPath;
            try
            {
                npmPath = NpmHelpers.GetPathToNpm(
                    nodejsProject != null ?
                        Nodejs.GetPathToNodeExecutableFromEnvironment()
                        : null);
            }
            catch (NpmNotFoundException)
            {
                return ExecutionResult.Failure;
            }

            var evaluator = window.Evaluator as NodejsReplEvaluator;

            Debug.Assert(evaluator != null, "How did we end up with an evaluator that's not the nodetools evaluator?");

            var npmReplRedirector = new NpmReplRedirector(evaluator);
            await NpmWorker.ExecuteNpmCommandAsync(
                npmPath,
                projectDirectoryPath,
                new[] { npmArguments },
                redirector: npmReplRedirector);

            if (npmReplRedirector.HasErrors)
            {
                window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.NpmReplCommandCompletedWithErrors, arguments));
            }
            else
            {
                window.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.NpmSuccessfullyCompleted, arguments));
            }

            return ExecutionResult.Success;
        }

        public override string Description => Resources.NpmExecuteCommand;

        public override string Command => "npm";

        internal sealed class NpmReplRedirector : Redirector
        {
            internal const string ErrorAnsiColor = "\x1b[31;1m";
            internal const string WarnAnsiColor = "\x1b[33;22m";
            internal const string NormalAnsiColor = "\x1b[39;49m";

            private const string ErrorText = "npm ERR!";
            private const string WarningText = "npm WARN";

            private readonly NodejsReplEvaluator evaluator;

            public NpmReplRedirector(NodejsReplEvaluator evaluator)
            {
                this.evaluator = evaluator;
                this.HasErrors = false;
            }

            public bool HasErrors { get; private set; }

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

                this.evaluator.WriteLine(outputString);
                Debug.WriteLine(decodedString, "REPL npm");
            }

            public override void WriteErrorLine(string line)
            {
                this.evaluator.WriteError(line);
            }
        }
    }
}
