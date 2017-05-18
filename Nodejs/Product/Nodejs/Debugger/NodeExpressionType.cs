// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    [Flags]
    internal enum NodeExpressionType
    {
        None = 0,

        /// <summary>
        /// Defines whether expression is property.
        /// </summary>
        Property = 0x1,

        /// <summary>
        /// Defines whether expression is function.
        /// </summary>
        Function = 0x2,

        /// <summary>
        /// Defines whether expression is boolean type.
        /// </summary>
        Boolean = 0x4,

        /// <summary>
        /// Defines whether expression is private member.
        /// </summary>
        Private = 0x8,

        /// <summary>
        /// Defines whether property is expandable.
        /// </summary>
        Expandable = 0x10,

        /// <summary>
        /// Defines whether property is readonly.
        /// </summary>
        ReadOnly = 0x20,

        /// <summary>
        /// Defines whether property is string type.
        /// </summary>
        String = 0x40
    }
}
