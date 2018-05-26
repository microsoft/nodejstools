// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    [ExtensionUri(NodejsConstants.ExecutorUriString)]
    public class JavaScriptTestExecutor : ITestExecutor
    {
        private TestExecutorWorker worker;

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.EnsureInitialized();
            this.worker = new TestExecutorWorker(runContext, frameworkHandle);
            this.worker.RunTests(tests);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.EnsureInitialized();
            this.worker = new TestExecutorWorker(runContext, frameworkHandle);
            this.worker.RunTests(sources, new JavaScriptTestDiscoverer());
        }

        public void Cancel()
        {
            this.worker?.Cancel();
        }

        private void EnsureInitialized()
        {
            if (JavaScriptTestDiscoverer.AssemblyResolver == null)
            {
                JavaScriptTestDiscoverer.AssemblyResolver = new AssemblyResolver();
            }
        }
    }
}
