// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudioTools.Project
{
    public interface IPublishFile
    {
        /// <summary>
        /// Returns the source file that should be copied from.
        /// </summary>
        string SourceFile
        {
            get;
        }

        /// <summary>
        /// Returns the relative path for the destination file.
        /// </summary>
        string DestinationFile
        {
            get;
        }
    }
}

