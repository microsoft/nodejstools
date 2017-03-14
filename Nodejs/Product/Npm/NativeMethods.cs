// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Npm
{
    internal static class NativeMethods
    {
        public const int MAX_PATH = 260; // windef.h	
        public const int MAX_FOLDER_PATH = MAX_PATH - 12;   // folders need to allow 8.3 filenames, so MAX_PATH - 12
    }
}

