// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    public sealed class NodejsTestInfo
    {
        public NodejsTestInfo(string testPath, string testName, string testFramework, int line, int column, string projectRootDir)
        {
            Debug.Assert(testPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || testPath.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase));

            var testFileRelative = CommonUtils.GetRelativeFilePath(projectRootDir, testPath);

            this.TestFile = testFileRelative;
            this.TestPath = testPath;
            this.TestName = testName;
            this.TestFramework = testFramework;
            this.SourceLine = line;
            this.SourceColumn = column;
        }

        public string FullyQualifiedName => $"{this.TestFile}::{this.TestName}::{this.TestFramework}";

        /// <summary>
        /// Project root relative path to the test file.
        /// </summary>
        public string TestFile { get; }

        /// <summary>
        /// Full path to the test file.
        /// </summary>
        public string TestPath { get; }

        public string TestName { get; }

        public string TestFramework { get; }

        public int SourceLine { get; }

        public int SourceColumn { get; }
    }
}
