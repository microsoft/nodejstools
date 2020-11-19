// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools;
using Microsoft.NodejsTools.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using static Microsoft.NodejsTools.TestAdapter.TestFrameworks.TestFramework;

namespace TestAdapter.Tests
{
    public class TestCaseFactory
    {
        private readonly SupportedFramework testFramework;
        private readonly string workingDir;
        private readonly string source;
        private readonly string configDirPath;

        public TestCaseFactory(SupportedFramework testFramework, string workingDir, string source, string configDirPath)
        {
            this.testFramework = testFramework;
            this.workingDir = workingDir;
            this.source = source;
            this.configDirPath = configDirPath;
        }

        public TestCase GetTestCase(string fullyQualifiedName, string displayName, int lineNumber, string filePath)
        {
            var testCase = new TestCase()
            {
                FullyQualifiedName = fullyQualifiedName,
                DisplayName = displayName,
                LineNumber = lineNumber,
                ExecutorUri = NodejsConstants.ExecutorUri,
                Source = this.source,
                CodeFilePath = filePath,
            };

            testCase.SetPropertyValue(JavaScriptTestCaseProperties.TestFramework, this.testFramework.ToString());
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, this.workingDir);
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.ProjectRootDir, this.workingDir);
            // TODO: We might only want to check that this is not empty. The value changes depending on the environment.
            //testCase.SetPropertyValue(JavaScriptTestCaseProperties.NodeExePath, nodeExePath);
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.TestFile, filePath);
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.ConfigDirPath, this.configDirPath);

            return testCase;
        }
    }
}
