// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    // We require to put a non-existent file extension to avoid duplicate discovery executions.
    [FileExtension("NTVS_NonExistentFileExtension")]
    public sealed class DefaultTestDiscoverer : ProjectTestDiscoverer, ITestDiscoverer
    {
        public override string TestDiscovererName => nameof(DefaultTestDiscoverer);
    }
}
