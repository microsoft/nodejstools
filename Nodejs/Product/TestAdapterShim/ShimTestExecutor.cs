// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    [ExtensionUri(TestExecutor.ExecutorUriString)]
    public class ShimTestExecutor : ITestExecutor
    {
        private TestExecutor testExecutor;

        public void Cancel()
        {
            this.EnsureInitialized();
            this.testExecutor.Cancel();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.EnsureInitialized();
            this.testExecutor.RunTests(tests, runContext, frameworkHandle);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.EnsureInitialized();
            this.testExecutor.RunTests(sources, runContext, frameworkHandle);
        }

        private void EnsureInitialized()
        {
            if (ShimTestDiscoverer.AssemblyResolver == null)
            {
                ShimTestDiscoverer.AssemblyResolver = new AssemblyResolver();
            }
            if (this.testExecutor == null)
            {
                this.testExecutor = new TestExecutor();
            }
        }
    }
}
