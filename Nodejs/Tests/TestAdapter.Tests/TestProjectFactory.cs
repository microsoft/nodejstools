// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using static Microsoft.NodejsTools.TestAdapter.TestFrameworks.TestFramework;

namespace TestAdapter.Tests
{
    public class TestProjectFactory
    {
        public enum ProjectName
        {
            NodeAppWithTestsConfiguredOnProject,
            NodeAppWithTestsConfiguredPerFile,
            NodeAppWithAngularTests,
        }

        private readonly ProjectName projectName;
        private string projectNameString => Enum.GetName(typeof(ProjectName), this.projectName);

        public TestProjectFactory(ProjectName projectName)
        {
            this.projectName = projectName;
        }

        public string GetProjectDirPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), $@"..\..\..\..\MockProjects\{projectNameString}");
        }

        public string GetProjectFilePath()
        {
            switch (this.projectName)
            {
                case ProjectName.NodeAppWithTestsConfiguredOnProject:
                case ProjectName.NodeAppWithTestsConfiguredPerFile:
                case ProjectName.NodeAppWithAngularTests:
                    return Path.Combine(this.GetProjectDirPath(), $"{projectNameString}.njsproj");
            }

            throw new ArgumentException();
        }

        public List<TestCase> GetTestCases()
        {
            switch (this.projectName)
            {
                case ProjectName.NodeAppWithTestsConfiguredOnProject:
                    return new List<TestCase>(this.GetTestCases(SupportedFramework.Mocha));

                case ProjectName.NodeAppWithTestsConfiguredPerFile:
                    var result = new List<TestCase>();
                    result.AddRange(this.GetTestCases(SupportedFramework.Jasmine));
                    result.AddRange(this.GetTestCases(SupportedFramework.ExportRunner));
                    result.AddRange(this.GetTestCases(SupportedFramework.Jest));
                    result.AddRange(this.GetTestCases(SupportedFramework.Mocha));
                    result.AddRange(this.GetTestCases(SupportedFramework.Tape));

                    return result;

                case ProjectName.NodeAppWithAngularTests:
                    return new List<TestCase>(this.GetTestCases(SupportedFramework.Angular));

            };

            throw new InvalidOperationException();
        }

        private List<TestCase> GetTestCases(SupportedFramework testFramework)
        {
            var configPath = this.projectName == ProjectName.NodeAppWithAngularTests
                ? this.GetProjectDirPath()
                : null;

            var testCaseFactory = new TestCaseFactory(
                testFramework,
                this.GetProjectDirPath(),
                this.GetProjectFilePath(),
                configPath);

            switch (testFramework)
            {
                case SupportedFramework.Jasmine:
                    return new List<TestCase>()
                    {
                        testCaseFactory.GetTestCase("JasmineUnitTest.js::Test Suite 1::Test 1", "Test 1", 1, Path.Combine(this.GetProjectDirPath(), "JasmineUnitTest.js")),
                        testCaseFactory.GetTestCase("JasmineUnitTest.js::Test Suite 1::Test 2", "Test 2", 1, Path.Combine(this.GetProjectDirPath(), "JasmineUnitTest.js")),
                    };
                case SupportedFramework.ExportRunner:
                    return new List<TestCase>()
                    {
                        testCaseFactory.GetTestCase("ExportRunnerUnitTest.js::global::Test 1", "Test 1", 1, Path.Combine(this.GetProjectDirPath(), "ExportRunnerUnitTest.js")),
                        testCaseFactory.GetTestCase("ExportRunnerUnitTest.js::global::Test 2", "Test 2", 1, Path.Combine(this.GetProjectDirPath(), "ExportRunnerUnitTest.js")),
                    };
                case SupportedFramework.Jest:
                    return new List<TestCase>()
                    {
                        testCaseFactory.GetTestCase("JestUnitTest.js::Test Suite 1::Test 1 - This shouldn't fail", "Test 1 - This shouldn't fail", 3, Path.Combine(this.GetProjectDirPath(), "JestUnitTest.js")),
                        testCaseFactory.GetTestCase("JestUnitTest.js::Test Suite 1::Test 2 - This should fail", "Test 2 - This should fail", 7, Path.Combine(this.GetProjectDirPath(), "JestUnitTest.js")),
                    };
                case SupportedFramework.Mocha:
                    return new List<TestCase>()
                    {
                        testCaseFactory.GetTestCase("MochaUnitTest.js::Test Suite 1::Test 1", "Test 1", 1, Path.Combine(this.GetProjectDirPath(), "MochaUnitTest.js")),
                        testCaseFactory.GetTestCase("MochaUnitTest.js::Test Suite 1::Test 2", "Test 2", 1, Path.Combine(this.GetProjectDirPath(), "MochaUnitTest.js")),
                    };
                case SupportedFramework.Tape:
                    return new List<TestCase>()
                    {
                        testCaseFactory.GetTestCase("TapeUnitTest.js::global::Test A", "Test A", 1, Path.Combine(this.GetProjectDirPath(), "TapeUnitTest.js")),
                        testCaseFactory.GetTestCase("TapeUnitTest.js::global::Test B", "Test B", 1, Path.Combine(this.GetProjectDirPath(), "TapeUnitTest.js")),
                    };
                case SupportedFramework.Angular:
                    return new List<TestCase>()
                    {
                        testCaseFactory.GetTestCase(@"src\app\app.component.spec.ts::AppComponent::should create the app","should create the app", 14, Path.Combine(this.GetProjectDirPath(), @"src\app\app.component.spec.ts")),
                        testCaseFactory.GetTestCase(@"src\app\app.component.spec.ts::AppComponent::should have as title 'my-app'","should have as title 'my-app'", 20, Path.Combine(this.GetProjectDirPath(), @"src\app\app.component.spec.ts")),
                        testCaseFactory.GetTestCase(@"src\app\app.component.spec.ts::AppComponent::should render title","should render title", 26, Path.Combine(this.GetProjectDirPath(), @"src\app\app.component.spec.ts")),
                        testCaseFactory.GetTestCase(@"src\app\customTest.spec.ts::CustomTest::should succeed","should succeed", 4, Path.Combine(this.GetProjectDirPath(), @"src\app\customTest.spec.ts")),
                        testCaseFactory.GetTestCase(@"src\app\customTest.spec.ts::CustomTest::should fail","should fail", 8, Path.Combine(this.GetProjectDirPath(), @"src\app\customTest.spec.ts")),
                    };
            }

            throw new ArgumentException();
        }
    }
}
