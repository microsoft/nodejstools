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
            replacementsDictionary.Add("$typescriptversion$", "");
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

        //private static string GetLatestAvailableTypeScriptVersionFromSetup()
        //{
        //    var setupCompositionService = (IVsSetupCompositionService)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsSetupCompositionService));

        //    // Populate the package status
        //    setupCompositionService.GetSetupPackagesInfo(0, null, out var sizeNeeded);

        //    if (sizeNeeded > 0)
        //    {
        //        var packages = new IVsSetupPackageInfo[sizeNeeded];
        //        setupCompositionService.GetSetupPackagesInfo(sizeNeeded, packages, out _);

        //        var typeScriptSdkPackageGroups = packages.Where(p => p.PackageId.StartsWith(tsSdkSetupPackageIdPrefix, StringComparison.OrdinalIgnoreCase))
        //            .GroupBy(p => p.CurrentState, p => p.PackageId);

        //        var installed = typeScriptSdkPackageGroups.Where(g => g.Key == (uint)__VsSetupPackageState.INSTALL_PACKAGE_PRESENT);
        //        if (installed.Any())
        //        {
        //            return GetVersion(installed.First());
        //        }

        //        // There is an issue in the installer where components aren't registered as 'Present', however they do show up as unknown.
        //        // So use that as a fallback.
        //        var unknown = typeScriptSdkPackageGroups.Where(g => g.Key == (uint)__VsSetupPackageState.INSTALL_PACKAGE_UNKNOWN);
        //        if (unknown.Any())
        //        {
        //            return GetVersion(unknown.First());
        //        }

        //        // This should not happen, since TS should be installed as a required component, however we should guard against
        //        // bugs in the installer, and use a good default for the user. 
        //        Debug.Fail("Failed to find a valid install of the TypeScript SDK.");
        //    }

        //    return "";

        //    string GetVersion(IEnumerable<string> installed)
        //    {
        //        return installed.Select(p => p.Substring(tsSdkSetupPackageIdPrefix.Length, p.Length - tsSdkSetupPackageIdPrefix.Length))
        //                 .OrderByDescending(v => v)
        //                 .First();
        //    }
        //}
    }
}
