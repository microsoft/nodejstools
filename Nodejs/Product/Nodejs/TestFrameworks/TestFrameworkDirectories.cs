// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.NodejsTools.TestFrameworks
{
    internal class TestFrameworkDirectories
    {
        public const string ExportRunnerFramework = "ExportRunner";
        private const string TestFrameworksDirectory = "TestFrameworks";

        private readonly Dictionary<string, string> _frameworkDirectories;

        public TestFrameworkDirectories()
        {
            this._frameworkDirectories = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var directory in Directory.GetDirectories(GetBaseTestframeworkFolder()))
            {
                var name = Path.GetFileName(directory);
                this._frameworkDirectories.Add(name, directory);
            }
            string defaultFx;
            this._frameworkDirectories.TryGetValue(ExportRunnerFramework, out defaultFx);
            if (defaultFx == null)
            {
                throw new InvalidOperationException("Missing generic test framework");
            }
        }

        public List<string> GetFrameworkNames()
        {
            return new List<string>(this._frameworkDirectories.Keys);
        }

        public List<string> GetFrameworkDirectories()
        {
            return new List<string>(this._frameworkDirectories.Values);
        }

        private static string GetBaseTestframeworkFolder()
        {
            var installFolder = GetExecutingAssemblyPath();
            var baseDirectory = Path.Combine(installFolder, TestFrameworksDirectory);
#if DEBUG
            // To allow easier debugging of the test adapter, try to use the local directory as a fallback.
            baseDirectory = Directory.Exists(baseDirectory) ? baseDirectory : Path.Combine(Directory.GetCurrentDirectory(), TestFrameworksDirectory);
#endif
            return baseDirectory;
        }

        private static string GetExecutingAssemblyPath()
        {
            var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}

