// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal sealed class TestReceiver : ITestCaseDiscoverySink
    {
        public readonly List<TestCase> Tests = new List<TestCase>();

        public void SendTestCase(TestCase discoveredTest)
        {
            this.Tests.Add(discoveredTest);
        }
    }
}
