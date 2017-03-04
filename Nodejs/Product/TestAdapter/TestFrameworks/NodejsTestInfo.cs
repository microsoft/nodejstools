// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal class NodejsTestInfo
    {
        public NodejsTestInfo(string fullyQualifiedName)
        {
            string[] parts = fullyQualifiedName.Split(new string[] { "::" }, StringSplitOptions.None);
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
        public string ModulePath { get; private set; }

        public string TestName { get; private set; }

        public string TestFramework { get; private set; }

        public int SourceLine { get; private set; }

        public int SourceColumn { get; private set; }
    }
}

