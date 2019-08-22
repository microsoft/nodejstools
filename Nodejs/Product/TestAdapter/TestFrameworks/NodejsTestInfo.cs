// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    public sealed class NodejsTestInfo
    {
        public NodejsTestInfo(string testPath, string suite, string testName, string testFramework, int line, int column, string projectRootDir)
        {
            Debug.Assert(testPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || testPath.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase));

            var testFileRelative = CommonUtils.GetRelativeFilePath(projectRootDir, testPath);

            this.TestFile = testFileRelative;
            this.TestPath = testPath;
            this.TestName = testName;
            this.TestFramework = testFramework;
            this.SourceLine = line;
            this.SourceColumn = column;
            this.Suite = suite;
        }

        public string FullyQualifiedName
        {
            get
            {
                // If no suite is defined, it on the "global" space.
                var suite = string.IsNullOrWhiteSpace(this.Suite) ? "global" : this.Suite;

                // Only three levels are allowed on vstest.
                return $"{this.TestFile}::{suite}::{this.TestName}";
            }
        }

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

        public string Suite { get; }
    }
}
