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
                throw new ArgumentException("Invalid fully qualified test name");
            }
            ModulePath = parts[0];
            TestName = parts[1];
            TestFramework = parts[2];
        }

        public NodejsTestInfo(string modulePath, string testName, string testFramework, int line, int column)
        {
            ModulePath = modulePath;
            TestName = testName;
            TestFramework = testFramework;
            SourceLine = line;
            SourceColumn = column;
        }

        public string FullyQualifiedName
        {
            get
            {
                return ModulePath + "::" + TestName + "::" + TestFramework;
            }
        }
        public string ModulePath { get; }

        public string TestName { get; }

        public string TestFramework { get; }

        public int SourceLine { get; }

        public int SourceColumn { get; }
    }
}
