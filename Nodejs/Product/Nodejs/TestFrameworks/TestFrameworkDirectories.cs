// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.TestFrameworks
{
    internal static class TestFrameworkDirectories
    {
        public const string ExportRunnerFrameworkName = "ExportRunner";
        private const string TestFrameworksFolderName = "TestFrameworks";
        private const string TestAdapterFolderName = "TestAdapter";

        public static string[] GetFrameworkNames()
        {
            var testFrameworkRoot = GetTestframeworkFolderRoot();
            if (!Directory.Exists(testFrameworkRoot))
            {
                throw new InvalidOperationException($"Unable to find test framework folder. Tried: \"{testFrameworkRoot}\"");
            }

            return Directory.EnumerateDirectories(testFrameworkRoot).Select(Path.GetFileName).ToArray();
        }

        public static string[] GetFrameworkDirectories(string testFrameworkRoot = null)
        {
            testFrameworkRoot = testFrameworkRoot ?? GetTestframeworkFolderRoot();

            if (!Directory.Exists(testFrameworkRoot))
            {
                throw new InvalidOperationException($"Unable to find test framework folder. Tried: \"{testFrameworkRoot}\"");
            }

            return Directory.EnumerateDirectories(testFrameworkRoot).ToArray();
        }

        private static string GetTestframeworkFolderRoot()
        {
            // This class is used in 3 different assemblies, installed in 3 locations:
            //
            // "<VSROOT>\Common7\IDE\Extensions\Microsoft\NodeJsTools\NodeJsTools\Microsoft.NodejsTools.dll",
            // "<VSROOT>\Common7\IDE\Extensions\Microsoft\NodeJsTools\TestAdapter\Microsoft.NodejsTools.TestAdapter.dll"
            // and <NuGetRoot>\microsoft.unittest.javascript\<version>\build\netstandard2.0\Microsoft.JavaScript.TestAdapter.dll

            string testAdapterAssemblyFolder;

            var currentAssembly = typeof(TestFrameworkDirectories).Assembly;

            if (currentAssembly.FullName.StartsWith("Microsoft.NodejsTools.TestAdapter", StringComparison.OrdinalIgnoreCase))
            {
                testAdapterAssemblyFolder = Path.GetDirectoryName(currentAssembly.Location);
            }
            else if (currentAssembly.FullName.StartsWith("Microsoft.NodejsTools", StringComparison.OrdinalIgnoreCase))
            {
                var NodeJsToolsFolder = Path.GetDirectoryName(currentAssembly.Location);
                testAdapterAssemblyFolder = Path.Combine(Path.GetDirectoryName(NodeJsToolsFolder), TestAdapterFolderName);
#if DEBUG
                // when debugging the experimental instance the folders are slightly different
                // the Test frameworks are here: %localappdata%\Microsoft\VisualStudio\15.0_2de0f20fExp\Extensions\Microsoft\Node.js Test Adapter\42.42.42.42\TestFrameworks
                // Node JS Tools folder is here: %localappdata%\Microsoft\VisualStudio\15.0_2de0f20fExp\Extensions\Microsoft\Node.js Tools\42.42.42.42
                if (!Directory.Exists(Path.Combine(testAdapterAssemblyFolder, TestFrameworksFolderName)))
                {
                    var version = Path.GetFileName(NodeJsToolsFolder);
                    var microsoftRoot = NodeJsToolsFolder.Substring(0, NodeJsToolsFolder.Length - version.Length - "\\Node.js Tools".Length);

                    Debug.Assert(microsoftRoot.EndsWith("\\"));

                    return Path.Combine(microsoftRoot, "Node.js Test Adapter", version, TestFrameworksFolderName);
                }
#endif
            }
            else if (currentAssembly.FullName.StartsWith("Microsoft.JavaScript.TestAdapter", StringComparison.OrdinalIgnoreCase))
            {
                // This case only happens when running tests on a .Net Core project from Visual Studio's Test Explorer. 
                // For the dotnet cli, the setting should have already been set on Microsoft.UnitTest.Javascript.targets file.
                testAdapterAssemblyFolder = Path.Combine(Path.GetDirectoryName(currentAssembly.Location), "..", "_common");
            }
            else
            {
                throw new InvalidOperationException($"Called from unexepected assembly: '{currentAssembly.FullName}'.");
            }

            return Path.Combine(testAdapterAssemblyFolder, TestFrameworksFolderName);
        }
    }
}
