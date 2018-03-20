// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal class NodejsTestInfo
    {
        public NodejsTestInfo(string fullyQualifiedName, string modulePath)
        {
            var parts = fullyQualifiedName.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 3)
            {
                throw new ArgumentException($"Invalid fully qualified test name. '{fullyQualifiedName}'", nameof(fullyQualifiedName));
            }

            this.ModulePath = modulePath;

            this.ModuleName = parts[0];
            this.TestName = parts[1];
            this.TestFramework = parts[2];
        }

        public NodejsTestInfo(string modulePath, string testName, string testFramework, int line, int column, string projectRootDir)
        {
            var relativePath = CommonUtils.GetRelativeFilePath(projectRootDir, modulePath);
            var moduleName = Regex.Replace(relativePath, @"[ \\/]", "_"); // use regex to replace spaces and slashes with '_' 

            var fileName = Path.GetFileNameWithoutExtension(modulePath);

            this.ModulePath = modulePath;
            this.ModuleName = moduleName;
            this.TestName = testName;
            this.TestFramework = testFramework;
            this.SourceLine = line;
            this.SourceColumn = column;
        }

        public string FullyQualifiedName => $"{this.ModuleName}::{this.TestName}::{this.TestFramework}";

        public string ModulePath { get; }

        public string ModuleName { get; }

        public string TestName { get; }

        public string TestFramework { get; }

        public int SourceLine { get; }

        public int SourceColumn { get; }
    }
}
