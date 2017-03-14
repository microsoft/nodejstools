// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;

using MSBuild = Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TypeScript
{
    internal static class TypeScriptHelpers
    {
        internal static bool IsTypeScriptFile(string filename)
        {
            return String.Equals(Path.GetExtension(filename), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase);
        }

        internal static string GetTypeScriptBackedJavaScriptFile(MSBuild.Project project, string pathToFile)
        {
            var typeScriptOutDir = project.GetPropertyValue(NodeProjectProperty.TypeScriptOutDir);
            return GetTypeScriptBackedJavaScriptFile(project.DirectoryPath, typeScriptOutDir, pathToFile);
        }

        internal static string GetTypeScriptBackedJavaScriptFile(IVsProject project, string pathToFile)
        {
            //Need to deal with the format being relative and explicit
            var props = (IVsBuildPropertyStorage)project;
            String outDir;
            ErrorHandler.ThrowOnFailure(props.GetPropertyValue(NodeProjectProperty.TypeScriptOutDir, null, 0, out outDir));

            var projHome = GetProjectHome(project);

            return GetTypeScriptBackedJavaScriptFile(projHome, outDir, pathToFile);
        }

        private static string GetTypeScriptBackedJavaScriptFile(string projectHome, string typeScriptOutDir, string pathToFile)
        {
            var jsFilePath = Path.ChangeExtension(pathToFile, NodejsConstants.JavaScriptExtension);

            if (String.IsNullOrEmpty(typeScriptOutDir))
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

        private static string GetProjectHome(IVsProject project)
        {
            Debug.Assert(project != null);
            var hier = (IVsHierarchy)project;
            object extObject;
            ErrorHandler.ThrowOnFailure(hier.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out extObject
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
            var projHome = props.Item("ProjectHome");
            if (projHome == null)
            {
                return null;
            }

            return projHome.Value as string;
        }
    }
}

