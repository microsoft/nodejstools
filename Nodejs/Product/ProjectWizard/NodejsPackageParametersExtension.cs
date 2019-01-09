// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard
{
    internal class NodejsPackageParametersExtension : IWizard
    {
        //private const string tsSdkSetupPackageIdPrefix = "Microsoft.VisualStudio.Component.TypeScript.";

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var projectName = replacementsDictionary["$projectname$"];
            replacementsDictionary.Add("$npmsafeprojectname$", NormalizeNpmPackageName(projectName));
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            return;
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            return;
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            return;
        }

        public void RunFinished()
        {
            return;
        }

        private const int NpmPackageNameMaxLength = 214;

        /// <summary>
        /// Normalize a project name to be a valid Npm package name: https://docs.npmjs.com/files/package.json#name
        /// </summary>
        /// <param name="projectName">Name of a VS project.</param>
        private static string NormalizeNpmPackageName(string projectName)
        {
            // Remove all leading url-invalid, underscore, and period characters
            var npmProjectNameTransform = Regex.Replace(projectName, "^[^a-zA-Z0-9-~]*", string.Empty);

            // Replace all invalid characters with a dash
            npmProjectNameTransform = Regex.Replace(npmProjectNameTransform, "[^a-zA-Z0-9-_~.]", "-");

            // Insert hyphens between camelcased sections.
            npmProjectNameTransform = Regex.Replace(npmProjectNameTransform, "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();

            return npmProjectNameTransform.Substring(0, Math.Min(npmProjectNameTransform.Length, NpmPackageNameMaxLength));
        }
    }
}
