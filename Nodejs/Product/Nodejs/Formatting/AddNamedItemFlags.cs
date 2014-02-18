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

namespace Microsoft.NodejsTools.Formatting {

    [Flags]
    enum AddNamedItemFlags {
        IsVisible = 0x00000002,
        IsSource = 0x00000004,
        GlobalMembers = 0x00000008,
        IsPersistent = 0x00000040,
        CodeOnly = 0x00000200,
        NoCode = 0x00000400
    }
}
