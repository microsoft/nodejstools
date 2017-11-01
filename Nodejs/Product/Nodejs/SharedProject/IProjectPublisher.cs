// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Implements a publisher which handles publishing the list of files to a destination.
    /// </summary>
    public interface IProjectPublisher
    {
        /// <summary>
        /// Publishes the files listed in the given project to the provided URI.
        /// 
        /// This function should return when publishing is complete or throw an exception if publishing fails.
        /// </summary>
        /// <param name="project">The project to be published.</param>
        /// <param name="destination">The destination URI for the project.</param>
        void PublishFiles(IPublishProject project, Uri destination);

        /// <summary>
        /// Gets a localized description of the destination type (web site, file share, etc...)
        /// </summary>
        string DestinationDescription
        {
            get;
        }

        /// <summary>
        /// Gets the schema supported by this publisher - used to select which publisher will
        /// be used based upon the schema of the Uri provided by the user.
        /// </summary>
        string Schema
        {
            get;
        }
    }
}
