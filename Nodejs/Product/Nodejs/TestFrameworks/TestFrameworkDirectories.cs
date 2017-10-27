// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.TestFrameworks
{
    internal class TestFrameworkDirectories
    {
        public const string ExportRunnerFrameworkName = "ExportRunner";
        private const string TestFrameworksFolderName = "TestFrameworks";
        private const string TestAdapterFolderName = "TestAdapter";

        private readonly Dictionary<string, string> frameworkDirectories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string TestFrameworksFolder1 => TestFrameworksFolderName;

        public TestFrameworkDirectories()
        {
            var testFrameworkRoot = GetTestframeworkFolderRoot();
            if (!Directory.Exists(testFrameworkRoot))
            {
                throw new InvalidOperationException($"Unable to find test framework folder. Tried: \"{testFrameworkRoot}\"");
            }

            foreach (var directory in Directory.GetDirectories(testFrameworkRoot))
            {
                var name = Path.GetFileName(directory);
                this.frameworkDirectories.Add(name, directory);
            }

            if (!this.frameworkDirectories.TryGetValue(ExportRunnerFrameworkName, out var defaultFx) || string.IsNullOrEmpty(defaultFx))
            {
                throw new InvalidOperationException("Missing generic test framework.");
            }
        }

        public List<string> GetFrameworkNames() => this.frameworkDirectories.Keys.ToList();

        public List<string> GetFrameworkDirectories() => this.frameworkDirectories.Values.ToList();

        private static string GetTestframeworkFolderRoot()
        {
            // This class is used in 2 different assemblies, installed in 2 locations:
            //
            // "C:\Program Files (x86)\Microsoft Visual Studio\Preview\Enterprise\Common7\IDE\Extensions\Microsoft\NodeJsTools\NodeJsTools\Microsoft.NodejsTools.dll"
            // and
            // "C:\Program Files (x86)\Microsoft Visual Studio\Preview\Enterprise\Common7\IDE\Extensions\Microsoft\NodeJsTools\TestAdapter\Microsoft.NodejsTools.TestAdapter.dll"
            //
            // However in both cases, we should just go up a folder to the nodejstools root, and then into the TestAdapter folder.

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
            }
            else
            {
                throw new InvalidOperationException($"Unable to find '{TestFrameworksFolderName}' folder.");
            }

            return Path.Combine(testAdapterAssemblyFolder, TestFrameworksFolderName);
        }
    }
}
