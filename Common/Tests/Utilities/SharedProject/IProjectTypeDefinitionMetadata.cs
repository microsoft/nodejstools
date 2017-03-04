// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Metadata interface for getting information about declared project kinds.
    /// MEF requires that this be public.
    /// </summary>
    public interface IProjectTypeDefinitionMetadata
    {
        string ProjectExtension { get; }
        string ProjectTypeGuid { get; }
        string CodeExtension { get; }

        [DefaultValue("")]
        string SampleCode { get; }
    }
}

