// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.VisualStudioTools.Project
{
    public interface IPublishProject
    {
        /// <summary>
        /// Gets the list of files which need to be published.
        /// </summary>
        IList<IPublishFile> Files
        {
            get;
        }

        /// <summary>
        /// Gets the root directory of the project.
        /// </summary>
        string ProjectDir
        {
            get;
        }

        /// <summary>
        /// Gets or sets the progress of the publishing.
        /// </summary>
        int Progress
        {
            get;
            set;
        }
    }
}

