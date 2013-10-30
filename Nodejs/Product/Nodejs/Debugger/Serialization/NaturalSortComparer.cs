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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Perfroms natural string compare.
    /// </summary>
    public sealed class NaturalStringComparer : IComparer<string> {
        public int Compare(string x, string y) {
            return SafeNativeMethods.StrCmpLogicalW(x, y);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }
    }
}