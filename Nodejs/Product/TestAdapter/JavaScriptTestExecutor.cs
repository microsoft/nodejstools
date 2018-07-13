// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
#if !NETSTANDARD2_0
            AssemblyResolver.SetupHandler();
#endif
            this.Cancel();

            this.worker = new TestExecutorWorker(runContext, frameworkHandle);
            this.worker.RunTests(tests);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
#if !NETSTANDARD2_0
            AssemblyResolver.SetupHandler();
#endif
            // If we have a source file specified in the runtContext we should use that, otherwise sources should've been set.
            // This happens in the case of .NET Core where the source is the output dll for the project, 
            // so we set the projectfile for the current project in the props file we import.

            var settings = new UnitTestSettings(runContext.RunSettings, runContext.IsBeingDebugged);

            var source = settings.TestSource;
            if (!string.IsNullOrEmpty(source))
            {
                sources = new[] { source };
            }

            this.Cancel();
            this.worker = new TestExecutorWorker(runContext, frameworkHandle);
            this.worker.RunTests(sources, new ProjectTestDiscoverer());
        }

        public void Cancel()
        {
            this.worker?.Cancel();
        }
    }
}
