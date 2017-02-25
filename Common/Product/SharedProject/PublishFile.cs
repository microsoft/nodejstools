/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
