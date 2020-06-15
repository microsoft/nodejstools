// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace Microsoft.NodejsTools.TestAdapter
{
    [Export(typeof(ITestMethodResolver))]
    internal class TestMethodResolver : ITestMethodResolver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TestContainerDiscoverer discoverer;

        [ImportingConstructor]
        public TestMethodResolver(
            [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            [Import]TestContainerDiscoverer discoverer)
        {
            this.serviceProvider = serviceProvider;
            this.discoverer = discoverer;
        }

        public Uri ExecutorUri => NodejsConstants.ExecutorUri;

        public string GetCurrentTest(string filePath, int line, int lineCharOffset)
        {
            // TODO: finish implementing this method with specified imports.
            return null;
        }
    }
}
