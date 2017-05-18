// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    /// <summary>
    /// Defines a node property type.
    /// </summary>
    internal enum NodePropertyType
    {
        Normal = 0,
        Field = 1,
        Constant = 2,
        Callbacks = 3,
        Handler = 4,
        Interceptor = 5,
        Transition = 6,
        Nonexistent = 7
    }
}
