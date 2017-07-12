// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.TestFrameworks
{
    internal class TestFrameworkDirectories
    {
        public const string ExportRunnerFramework = "ExportRunner";
        private const string TestFrameworksDirectory = "TestFrameworks";
        private const string TestAdapterDirectory = "TestAdapter";

        private readonly Dictionary<string, string> frameworkDirectories;

        public TestFrameworkDirectories()
        {
            this.frameworkDirectories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var testFrameworkRoot = GetTestframeworkFolderRoot();
            if(!Directory.Exists(testFrameworkRoot))
            {
                throw new InvalidOperationException("Unable to find test framework folder");
            }

            foreach (var directory in Directory.GetDirectories(testFrameworkRoot))
            {
                var name = Path.GetFileName(directory);
                this.frameworkDirectories.Add(name, directory);
            }

            if (!this.frameworkDirectories.TryGetValue(ExportRunnerFramework, out var defaultFx) || string.IsNullOrEmpty(defaultFx))
            {
                throw new InvalidOperationException("Missing generic test framework");
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

            var currentAssembly = typeof(TestFrameworkDirectories).Assembly;
            var currentAssemblyFolder = Path.GetDirectoryName(currentAssembly.Location);
            var nodejsRootFolder = Path.GetDirectoryName(currentAssemblyFolder);

            var baseDirectory = Path.Combine(nodejsRootFolder, TestAdapterDirectory, TestFrameworksDirectory);
#if DEBUG
            // To allow easier debugging of the test adapter, try to use the local directory as a fallback.
            baseDirectory = Directory.Exists(baseDirectory) ? baseDirectory : Path.Combine(Directory.GetCurrentDirectory(), TestFrameworksDirectory);
#endif
            return baseDirectory;
        }
    }
}
