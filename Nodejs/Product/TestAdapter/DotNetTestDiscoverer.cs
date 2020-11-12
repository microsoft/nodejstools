// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    // Keep in sync the method TypeScriptHelpers.IsSupportedTestProjectFile if there's a change on the supported projects.
    [FileExtension(NodejsConstants.CSharpProjectExtension), FileExtension(NodejsConstants.VisualBasicProjectExtension)]
    [DefaultExecutorUri(NodejsConstants.ExecutorUriString)]
    public sealed class DotNetTestDiscoverer : ProjectTestDiscoverer, ITestDiscoverer
    {
        public override string TestDiscovererName => nameof(DotNetTestDiscoverer);
    }
}
