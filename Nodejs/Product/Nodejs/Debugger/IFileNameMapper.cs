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
        /// Returns a local file name for a remote.
        /// </summary>
        /// <param name="remoteFileName">Remote file name.</param>
        /// <returns>Local file name.</returns>
        string GetLocalFileName(string remoteFileName);
    }
}