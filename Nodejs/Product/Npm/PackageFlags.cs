// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Npm
{
    [Flags]
    public enum PackageFlags
    {
        None = 0x0000,
        NotListedAsDependency = 0x0001,
        Missing = 0x0002,
        Dev = 0x0004,
        Optional = 0x0008,
        Bundled = 0x0010,
        VersionMismatch = 0x0100,
        Installed = 0x1000,
    }
}
