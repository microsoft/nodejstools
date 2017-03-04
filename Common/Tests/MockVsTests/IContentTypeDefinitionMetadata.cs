// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    public interface IContentTypeDefinitionMetadata
    {
        string Name { get; }

        [System.ComponentModel.DefaultValue(null)]
        IEnumerable<string> BaseDefinition { get; }
    }
}

