// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.NodejsTools.TestAdapter
{
    [FileExtension(".njsproj"), FileExtension("*.csproj"), FileExtension("*.vbproj")]
    [DefaultExecutorUri(NodejsConstants.ExecutorUriString)]
    public class ShimTestDiscoverer : ITestDiscoverer
    {
        private TestDiscoverer testDiscoverer;
        internal static AssemblyResolver AssemblyResolver { get; set; }

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (ShimTestDiscoverer.AssemblyResolver == null)
            {
                ShimTestDiscoverer.AssemblyResolver = new AssemblyResolver();
            }
            this.testDiscoverer = new TestDiscoverer();
            this.testDiscoverer.DiscoverTests(sources, discoveryContext, logger, discoverySink);
        }
    }
}
