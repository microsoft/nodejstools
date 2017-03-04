// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    /// <summary>
    /// Defines a node property attributes.
    /// </summary>
    [Flags]
    internal enum NodePropertyAttributes
    {
        None = 0,
        ReadOnly = 1,
        DontEnum = 2,
        DontDelete = 4
    }
}

