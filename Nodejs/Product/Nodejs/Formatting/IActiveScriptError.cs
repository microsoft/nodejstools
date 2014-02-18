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
using System.Runtime.InteropServices;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.NodejsTools.Formatting {

    [Guid("EAE1BA61-A4ED-11cf-8F20-00805F2CD064")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActiveScriptError {
        [PreserveSig]
        int GetExceptionInfo(
            /* [out] */ out EXCEPINFO pexcepinfo);

        [PreserveSig]
        int GetSourcePosition(
            /* [out] */ out uint pdwSourceContext,
            /* [out] */ out uint pulLineNumber,
            /* [out] */ out int plCharacterPosition);

        [PreserveSig]
        int GetSourceLineText(/* [out] */ out string pbstrSourceLine);
    }
}
