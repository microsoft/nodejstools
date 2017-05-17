// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudioTools.Project
{
    internal class PublishFile : IPublishFile
    {
        private readonly string _filename, _destFile;

        public PublishFile(string filename, string destFile)
        {
            this._filename = filename;
            this._destFile = destFile;
        }

        #region IPublishFile Members

        public string SourceFile => this._filename;
        public string DestinationFile => this._destFile;
        #endregion
    }
}

