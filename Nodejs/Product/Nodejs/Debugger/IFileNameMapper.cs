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

namespace Microsoft.NodejsTools.Debugger {
    interface IFileNameMapper {
        /// <summary>
        /// Add module to collection.
        /// </summary>
        /// <param name="fileName">File name.</param>
        void AddModuleName(string fileName);

        /// <summary>
        /// Perform match between local and remote module.
        /// </summary>
        /// <param name="remoteFileName">Remote file name.</param>
        /// <param name="localFileName">Local file name.</param>
        /// <returns>True if remote file is matched to local otherwise false.</returns>
        bool MatchFileName(string remoteFileName, string localFileName);

        /// <summary>
        /// Returns a local file name for a remote.
        /// </summary>
        /// <param name="remoteFileName">Remote file name.</param>
        /// <returns>Local file name.</returns>
        string GetLocalFileName(string remoteFileName);
    }
}