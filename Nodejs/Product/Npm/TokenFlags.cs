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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm {
    [Flags]
    public enum TokenFlags {
        None = 0x0000,
        Digits = 0x0001,
        Letters = 0x0002,
        Dots = 0x0004,
        Colons = 0x0008,
        Dashes = 0x0010,
        Whitespace = 0x0020,
        Newline = 0x0040,
        LeadingEquals = 0x0080,
        Other = 0x0100,
        ThatsAllFolks = 0x8000
    }
}
