// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using static Microsoft.NodejsTools.TestAdapter.TestFrameworks.TestFramework;

namespace TestAdapter.Tests
{
    public static class TestProjectFactory
    {
        public enum ProjectName
        {
            NodeAppWithTestsConfiguredOnProject,
            NodeAppWithTestsConfiguredPerFile,
            NodeAppWithAngularTests,
            // React Apps are created with lower-case names.
            reactappwithjesttestsjavascript,
            reactappwithjestteststypescript
        }

        private struct TestCaseOptions
        {
            public SupportedFramework TestFramework;
            public string Source;
            public string WorkingDir;
            public string ConfigDirPath;
        }

        public static string GetProjectDirPath(ProjectName projectName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), $@"..\..\..\..\MockProjects\{projectName}");
        }

        public static string GetProjectFilePath(ProjectName projectName)
        {
            switch (projectName)
            {
                case ProjectName.NodeAppWithTestsConfiguredOnProject:
                case ProjectName.NodeAppWithTestsConfiguredPerFile:
                case ProjectName.NodeAppWithAngularTests:
                    return Path.Combine(GetProjectDirPath(projectName), $"{projectName}.njsproj");
                case ProjectName.reactappwithjesttestsjavascript:
                case ProjectName.reactappwithjestteststypescript:
                    return Path.Combine(GetProjectDirPath(projectName), $"{projectName}.esproj");
            }

            throw new NotImplementedException($"ProjectName {projectName} has not been implemented.");
        }

        public static List<TestCase> GetTestCases(ProjectName projectName)
        {
            switch (projectName)
            {
                case ProjectName.NodeAppWithTestsConfiguredOnProject:
                    return new List<TestCase>(GetTestCases(projectName, SupportedFramework.Mocha));

                case ProjectName.NodeAppWithTestsConfiguredPerFile:
                    var result = new List<TestCase>();
                    result.AddRange(GetTestCases(projectName, SupportedFramework.Jasmine));
                    result.AddRange(GetTestCases(projectName, SupportedFramework.ExportRunner));
                    result.AddRange(GetTestCases(projectName, SupportedFramework.Jest));
                    result.AddRange(GetTestCases(projectName, SupportedFramework.Mocha));
                    result.AddRange(GetTestCases(projectName, SupportedFramework.Tape));

                    return result;

                case ProjectName.NodeAppWithAngularTests:
                    return new List<TestCase>(GetTestCases(projectName, SupportedFramework.Angular));

                case ProjectName.reactappwithjesttestsjavascript:
                    return new List<TestCase>(GetTestCases(projectName, SupportedFramework.Jest));

                case ProjectName.reactappwithjestteststypescript:
                    return new List<TestCase>(GetTestCases(projectName, SupportedFramework.Jest));

            };

            throw new NotImplementedException($"ProjectName {projectName} has not been implemented.");
        }

        private static List<TestCase> GetTestCases(ProjectName projectName, SupportedFramework testFramework)
        {
            var testCaseOptions = new TestCaseOptions()
            {
                TestFramework = testFramework,
                WorkingDir = GetProjectDirPath(projectName),
                Source = GetProjectFilePath(projectName),
                ConfigDirPath = projectName == ProjectName.NodeAppWithAngularTests ? GetProjectDirPath(projectName) : null
            };

            switch (testFramework)
            {
                case SupportedFramework.Jasmine:
                    {
                        var filePath = Path.Combine(GetProjectDirPath(projectName), "JasmineUnitTest.js");
                        return new List<TestCase>()
                        {
                            GetTestCase(testCaseOptions, "JasmineUnitTest.js::Test Suite 1::Test 1", "Test 1", 1, filePath),
                            GetTestCase(testCaseOptions, "JasmineUnitTest.js::Test Suite 1::Test 2", "Test 2", 1, filePath),
                        };
                    }
                case SupportedFramework.ExportRunner:
                    {
                        var filePath = Path.Combine(GetProjectDirPath(projectName), "ExportRunnerUnitTest.js");
                        return new List<TestCase>()
                        {
                            GetTestCase(testCaseOptions, "ExportRunnerUnitTest.js::global::Test 1", "Test 1", 1, filePath),
                            GetTestCase(testCaseOptions, "ExportRunnerUnitTest.js::global::Test 2", "Test 2", 1, filePath),
                        };
                    }
                case SupportedFramework.Jest:
                    {
                        // Using React out-of-the-box test file in case of a React project:
                        if(projectName == ProjectName.reactappwithjesttestsjavascript)
                        {
                            var filePath = Path.Combine(GetProjectDirPath(projectName), "src", "App.test.js");
                            return new List<TestCase>()
                            {
                                GetTestCase(testCaseOptions, "src\\App.test.js::global::renders learn react link", "renders learn react link", 5, filePath)
                            };
                        }
                        else if(projectName == ProjectName.reactappwithjestteststypescript)
                        {
                            var filePath = Path.Combine(GetProjectDirPath(projectName), "src", "App.test.tsx");
                            return new List<TestCase>()
                            {
                                GetTestCase(testCaseOptions, "src\\App.test.tsx::global::renders learn react link", "renders learn react link", 6, filePath)
                            };
                        }
                        else
                        {
                            var filePath = Path.Combine(GetProjectDirPath(projectName), "JestUnitTest.js");
                            return new List<TestCase>()
                            {
                                GetTestCase(testCaseOptions, "JestUnitTest.js::Test Suite 1::Test 1 - This shouldn't fail", "Test 1 - This shouldn't fail", 3, filePath),
                                GetTestCase(testCaseOptions, "JestUnitTest.js::Test Suite 1::Test 2 - This should fail", "Test 2 - This should fail", 7, filePath),
                            };
                        }
                    }
                case SupportedFramework.Mocha:
                    {
                        var filePath = Path.Combine(GetProjectDirPath(projectName), "MochaUnitTest.js");
                        return new List<TestCase>()
                        {
                            GetTestCase(testCaseOptions, "MochaUnitTest.js::Test Suite 1::Test 1", "Test 1", 1, filePath),
                            GetTestCase(testCaseOptions, "MochaUnitTest.js::Test Suite 1::Test 2", "Test 2", 1, filePath),
                        };
                    }
                case SupportedFramework.Tape:
                    {
                        var filePath = Path.Combine(GetProjectDirPath(projectName), "TapeUnitTest.js");
                        return new List<TestCase>()
                        {
                            GetTestCase(testCaseOptions, "TapeUnitTest.js::global::Test A", "Test A", 1, filePath),
                            GetTestCase(testCaseOptions, "TapeUnitTest.js::global::Test B", "Test B", 1, filePath),
                        };
                    }
                case SupportedFramework.Angular:
                    return new List<TestCase>()
                    {
                        GetTestCase(testCaseOptions, @"src\app\app.component.spec.ts::AppComponent::should create the app","should create the app", 14, Path.Combine(GetProjectDirPath(projectName), @"src\app\app.component.spec.ts")),
                        GetTestCase(testCaseOptions, @"src\app\app.component.spec.ts::AppComponent::should have as title 'my-app'","should have as title 'my-app'", 20, Path.Combine(GetProjectDirPath(projectName), @"src\app\app.component.spec.ts")),
                        GetTestCase(testCaseOptions, @"src\app\app.component.spec.ts::AppComponent::should render title","should render title", 26, Path.Combine(GetProjectDirPath(projectName), @"src\app\app.component.spec.ts")),
                        GetTestCase(testCaseOptions, @"src\app\customTest.spec.ts::CustomTest::should succeed","should succeed", 4, Path.Combine(GetProjectDirPath(projectName), @"src\app\customTest.spec.ts")),
                        GetTestCase(testCaseOptions, @"src\app\customTest.spec.ts::CustomTest::should fail","should fail", 8, Path.Combine(GetProjectDirPath(projectName), @"src\app\customTest.spec.ts")),
                    };
            }

            throw new NotImplementedException($"ProjectName {projectName} has not been implemented.");
        }

        private static TestCase GetTestCase(TestCaseOptions testCaseOptions, string fullyQualifiedName, string displayName, int lineNumber, string filePath)
        {
            var executorUri = new Uri("executor://NodejsTestExecutor/v1");

            var testCase = new TestCase()
            {
                FullyQualifiedName = fullyQualifiedName,
                DisplayName = displayName,
                LineNumber = lineNumber,
                ExecutorUri = executorUri,
                Source = testCaseOptions.Source,
                CodeFilePath = filePath,
            };

            testCase.SetPropertyValue(JavaScriptTestCaseProperties.TestFramework, testCaseOptions.TestFramework.ToString());
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.WorkingDir, testCaseOptions.WorkingDir);
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.ProjectRootDir, testCaseOptions.WorkingDir);
            // TODO: We might only want to check that this is not empty. The value changes depending on the environment.
            //testCase.SetPropertyValue(JavaScriptTestCaseProperties.NodeExePath, nodeExePath);
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.TestFile, filePath);
            testCase.SetPropertyValue(JavaScriptTestCaseProperties.ConfigDirPath, testCaseOptions.ConfigDirPath);

            return testCase;
        }
    }
}
