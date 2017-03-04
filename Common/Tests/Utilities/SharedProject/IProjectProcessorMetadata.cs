// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Interface for getting metadata for when we import our IProjectProcessor
    /// class.  MEF requires this to be public.
    /// </summary>
    public interface IProjectProcessorMetadata
    {
        string ProjectExtension { get; }
    }
}

