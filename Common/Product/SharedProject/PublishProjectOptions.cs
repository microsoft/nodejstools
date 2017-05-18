// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.VisualStudioTools.Project
{
    public sealed class PublishProjectOptions
    {
        private readonly IPublishFile[] _additionalFiles;
        private readonly string _destination;
        public static readonly PublishProjectOptions Default = new PublishProjectOptions(new IPublishFile[0]);

        public PublishProjectOptions(IPublishFile[] additionalFiles = null, string destinationUrl = null)
        {
            this._additionalFiles = additionalFiles ?? Default._additionalFiles;
            this._destination = destinationUrl;
        }

        public IList<IPublishFile> AdditionalFiles => this._additionalFiles;

        /// <summary>
        /// Gets an URL which overrides the project publish settings or returns null if no override is specified.
        /// </summary>
        public string DestinationUrl => this._destination;
    }
}
