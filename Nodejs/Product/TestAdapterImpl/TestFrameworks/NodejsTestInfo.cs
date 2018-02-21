// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal class NodejsTestInfo
    {
        public NodejsTestInfo(string fullyQualifiedName)
        {
            var parts = fullyQualifiedName.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 3)
            {
                throw new ArgumentException($"Invalid fully qualified test name. '{fullyQualifiedName}'", nameof(fullyQualifiedName));
            }
            this.ModulePath = parts[0];
            this.TestName = parts[1];
            this.TestFramework = parts[2];
        }

        public NodejsTestInfo(string modulePath, string testName, string testFramework, int line, int column)
        {
            this.ModulePath = modulePath;
            this.TestName = testName;
            this.TestFramework = testFramework;
            this.SourceLine = line;
            this.SourceColumn = column;
        }

        public string FullyQualifiedName => $"{this.ModulePath}::{this.TestName}::{this.TestFramework}";

        public string ModulePath { get; }

        public string TestName { get; }

        public string TestFramework { get; }

        public int SourceLine { get; }

        public int SourceColumn { get; }
    }
}
