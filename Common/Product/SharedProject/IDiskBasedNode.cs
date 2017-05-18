// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Represents a node which has a filename on disk, implemented by folder and file nodes.
    /// </summary>
    internal interface IDiskBasedNode
    {
        string Url
        {
            get;
        }

        void RenameForDeferredSave(string basePath, string baseNewPath);
    }
}

