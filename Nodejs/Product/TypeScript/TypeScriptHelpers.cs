// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
#if !NOVS
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
#endif
using Microsoft.VisualStudioTools;
#if !NETSTANDARD2_0
using MSBuild = Microsoft.Build.Evaluation;
#endif
namespace Microsoft.NodejsTools.TypeScript
{
    internal static class TypeScriptHelpers
    {
        internal static bool IsTypeScriptFile(string filename)
        {
            var extension = Path.GetExtension(filename);

            return StringComparer.OrdinalIgnoreCase.Equals(extension, NodejsConstants.TypeScriptExtension)
                || StringComparer.OrdinalIgnoreCase.Equals(extension, NodejsConstants.TypeScriptJsxExtension);
        }

        internal static bool IsJavaScriptFile(string filename)
        {
            var extension = Path.GetExtension(filename);

            return StringComparer.OrdinalIgnoreCase.Equals(extension, NodejsConstants.JavaScriptExtension);
        }

        internal static bool IsTsJsConfigJsonFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return StringComparer.OrdinalIgnoreCase.Equals(fileName, NodejsConstants.TsConfigJsonFile) ||
                StringComparer.OrdinalIgnoreCase.Equals(fileName, NodejsConstants.JsConfigJsonFile);
        }

        // Keep in sync the JavaScriptTestDiscoverer class if there's a change on the supported projects.
        internal static bool IsSupportedTestProjectFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            return StringComparer.OrdinalIgnoreCase.Equals(extension, NodejsConstants.NodejsProjectExtension);
        }

#if !NETSTANDARD2_0
        internal static string GetTypeScriptBackedJavaScriptFile(MSBuild.Project project, string pathToFile)
        {
            var typeScriptOutDir = project.GetPropertyValue(NodeProjectProperty.TypeScriptOutDir);
            return GetTypeScriptBackedJavaScriptFile(project.DirectoryPath, typeScriptOutDir, pathToFile);
        }
#endif
        internal static string GetTypeScriptBackedJavaScriptFile(string projectHome, string typeScriptOutDir, string pathToFile)
        {
            var jsFilePath = Path.ChangeExtension(pathToFile, NodejsConstants.JavaScriptExtension);

            if (string.IsNullOrEmpty(typeScriptOutDir))
            {
                //No setting for OutDir
                //  .js file is created next to .ts file
                return jsFilePath;
            }

            //Get the full path to outDir
            //  If outDir is rooted then outDirPath is going to be outDir ending with backslash
            var outDirPath = CommonUtils.GetAbsoluteDirectoryPath(projectHome, typeScriptOutDir);

            //Find the relative path to the file from projectRoot
            //  This folder structure will be mirrored in the TypeScriptOutDir
            var relativeJSFilePath = CommonUtils.GetRelativeFilePath(projectHome, jsFilePath);

            return Path.Combine(outDirPath, relativeJSFilePath);
        }

#if !NOVS
        internal static string GetTypeScriptBackedJavaScriptFile(IVsProject project, string pathToFile)
        {
            //Need to deal with the format being relative and explicit
            string outDir = null;

            if (project is IVsBuildPropertyStorage props)
            {
                // GetProperty can return this error code if the property doesn't exist
                const int ERR_XML_ATTRIBUTE_NOT_FOUND = unchecked((int)0x8004C738);

                try
                {
                    ErrorHandler.ThrowOnFailure(props.GetPropertyValue(NodeProjectProperty.TypeScriptOutDir, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out outDir));
                }
                catch (System.Runtime.InteropServices.COMException e) when (e.ErrorCode == ERR_XML_ATTRIBUTE_NOT_FOUND)
                {
                    return null;
                }
            }
            else
            {
                Debug.Fail($"Why is {nameof(project)} not of type {nameof(IVsBuildPropertyStorage)}?");
            }

            var projHome = GetProjectHome(project);

            if (projHome == null)
            {
                return null;
            }

            return GetTypeScriptBackedJavaScriptFile(projHome, outDir, pathToFile);
        }

        private static string GetProjectHome(IVsProject project)
        {
            Debug.Assert(project != null);
            var hier = (IVsHierarchy)project;
            ErrorHandler.ThrowOnFailure(hier.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out var extObject
            ));
            var proj = extObject as EnvDTE.Project;
            if (proj == null)
            {
                return null;
            }
            var props = proj.Properties;
            if (props == null)
            {
                return null;
            }

            try
            {
                var projHome = props.Item("ProjectHome");
                return projHome == null ? null : projHome.Value as string;
            }
            catch (ArgumentException)
            {
                // EnvDTE.Properties.Item may throw ArgumentException if the property is not found
                return null;
            }
        }
#endif
    }
}
