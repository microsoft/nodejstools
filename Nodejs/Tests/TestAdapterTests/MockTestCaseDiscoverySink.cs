// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace TestAdapterTests
{
    internal class MockTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        public readonly List<TestCase> Tests = new List<TestCase>();

        #region ITestCaseDiscoverySink Members

        public void SendTestCase(TestCase discoveredTest)
        {
            this.Tests.Add(discoveredTest);
        }

        #endregion
    }
}

