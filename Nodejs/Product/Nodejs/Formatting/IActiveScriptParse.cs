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
    [Guid("BB1A2AE2-A4F9-11cf-8F20-00805F2CD064")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActiveScriptParse {
        [PreserveSig]
        int InitNew();

        [PreserveSig]
        int AddScriptlet(
            /* [in] */ string pstrDefaultName,
            /* [in] */ string pstrCode,
            /* [in] */ string pstrItemName,
            /* [in] */ string pstrSubItemName,
            /* [in] */ string pstrEventName,
            /* [in] */ string pstrDelimiter,
            /* [in] */ uint dwSourceContextCookie,
            /* [in] */ uint ulStartingLineNumber,
            /* [in] */ uint dwFlags,
            /* [out] */ out string pbstrName,
            /* [out] */ out EXCEPINFO pexcepinfo);

        [PreserveSig]
        int ParseScriptText(
            /* [in] */ string pstrCode,
            /* [in] */ string pstrItemName,
            /* [in] */ [MarshalAs(UnmanagedType.IUnknown)] object punkContext,
            /* [in] */ string pstrDelimiter,
            /* [in] */ uint dwSourceContextCookie,
            /* [in] */ uint ulStartingLineNumber,
            /* [in] */ ParseScriptTextFlags dwFlags,
            /* [out] */ [Out] out object pvarResult,
            /* [out] */ IntPtr excepInfo);
    }

}
