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

    [Guid("DB01A1E3-A42B-11cf-8F20-00805F2CD064")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActiveScriptSite {
        [PreserveSig]
        int GetLCID(
            /* [out] */ out uint plcid);

        [PreserveSig]
        int GetItemInfo(
            /* [in] */ string pstrName, /* LPCOLESTR*/
            /* [in] */ GetItemInfoFlags dwReturnMask,
            /* [out] */ [MarshalAs(UnmanagedType.IUnknown)] out object ppiunkItem,
            /* [out] */ IntPtr ppti);

        [PreserveSig]
        int GetDocVersionString(
            /* [out] */ out string pbstrVersion);

        [PreserveSig]
        int OnScriptTerminate(
            /* [in] */ object pvarResult,
            /* [in] */ ref EXCEPINFO pexcepinfo);

        [PreserveSig]
        int OnStateChange(
            /* [in] */ ScriptState ssScriptState);

        [PreserveSig]
        int OnScriptError(/* [in] */ IActiveScriptError error);

        [PreserveSig]
        int OnEnterScript();

        [PreserveSig]
        int OnLeaveScript();
    }

}
