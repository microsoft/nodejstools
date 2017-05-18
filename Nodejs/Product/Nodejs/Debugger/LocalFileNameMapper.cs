// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Handles file name mapping while local debigging.
    /// </summary>
    internal sealed class LocalFileNameMapper : IFileNameMapper
    {
        public string GetLocalFileName(string remoteFileName)
        {
            return remoteFileName;
        }
    }
}
