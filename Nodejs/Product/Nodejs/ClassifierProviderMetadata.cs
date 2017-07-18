// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools
{
    /// <summary>
    /// Just used for our MEF import to get the metadata in a strongly
    /// typed way.
    /// </summary>
    public interface IClassifierProviderMetadata
    {
        IEnumerable<string> ContentTypes { get; }
    }
}
