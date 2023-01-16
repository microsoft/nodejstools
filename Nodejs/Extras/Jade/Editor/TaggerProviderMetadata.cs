// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools
{
    /// <summary>
    /// Just used for our MEF import to get the metadata in a strongly
    /// typed way.
    /// </summary>
    internal sealed class TaggerProviderMetadata
    {
        public readonly IEnumerable<string> ContentTypes;
        public readonly IEnumerable<Type> TagTypes;

        public TaggerProviderMetadata(IDictionary<string, object> values)
        {
            this.ContentTypes = (IEnumerable<string>)values["ContentTypes"];
            this.TagTypes = (IEnumerable<Type>)values["TagTypes"];
        }
    }
}
