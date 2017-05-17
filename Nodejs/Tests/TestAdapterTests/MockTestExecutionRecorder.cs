// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestAdapterTests
{
    internal class MockTestExecutionRecorder : IFrameworkHandle
    {
        public readonly List<TestResult> Results = new List<TestResult>();

        public bool EnableShutdownAfterTestRun
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public int LaunchProcessWithDebuggerAttached(string filePath, string workingDirectory, string arguments, IDictionary<string, string> environmentVariables)
        {
            return 0;
        }

        public void RecordResult(TestResult result)
        {
            this.Results.Add(result);
        }

        public void RecordAttachments(IList<AttachmentSet> attachmentSets)
        {
        }

        public void RecordEnd(TestCase testCase, TestOutcome outcome)
        {
        }

        public void RecordStart(TestCase testCase)
        {
        }

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
        }
    }
}

