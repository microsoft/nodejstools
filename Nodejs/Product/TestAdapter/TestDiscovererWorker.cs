// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    public class TestDiscovererWorker
    {
        private readonly string testSource;

        public TestDiscovererWorker(string testSource)
        {
            this.testSource = testSource;
        }

        public void DiscoverTests(ITestCaseDiscoverySink discoverySink)
        {

        }
    }
}
