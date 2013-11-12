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

namespace Microsoft.NodejsTools.Npm{
    [Flags]
    public enum PackageFlags{
        NotListedAsDependency = 0x0001,
        Missing = 0x0002,
        Dev = 0x0004,
        Optional = 0x0008,
        Bundled = 0x0010,
        VersionMismatch = 0x0100,
        Installed = 0x1000,
    }
}