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

using System;

namespace Microsoft.NodejsTools.Debugger {
    [Flags]
    enum NodeExpressionType {
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