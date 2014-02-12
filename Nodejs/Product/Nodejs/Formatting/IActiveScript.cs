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
using System.Dynamic;
using System.IO;
using System.Runtime.InteropServices;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.NodejsTools.Formatting {
    [Guid("BB1A2AE1-A4F9-11cf-8F20-00805F2CD064")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActiveScript {
        [PreserveSig]
        int SetScriptSite(IActiveScriptSite pScriptSite);

        [PreserveSig]
        int GetScriptSite(
            /* [in] */ ref Guid iid,
            /* [iid_is][out] */ out object ppvObject);

        [PreserveSig]
        int SetScriptState(
            /* [in] */ ScriptState ss);

        [PreserveSig]
        int GetScriptState(
            /* [out] */ out ScriptState pssState);

        [PreserveSig]
        int Close();

        [PreserveSig]
        int AddNamedItem(
            /* [in] */ string pstrName, /* LPCOLESTR*/
            /* [in] */ AddNamedItemFlags dwFlags);

        [PreserveSig]
        int AddTypeLib(
            /* [in] */ ref Guid iid,
            /* [in] */ uint dwMajor,
            /* [in] */ uint dwMinor,
            /* [in] */ uint dwFlags);

        [PreserveSig]
        int GetScriptDispatch(
            /* [in] */ string pstrItemName, /*LPCOLESTR*/
            /* [out] */ [MarshalAs(UnmanagedType.IDispatch)] out object ppdisp);

        [PreserveSig]
        int GetCurrentuint(
            /* [out] */ uint pstidThread);

        [PreserveSig]
        int Getuint(
            /* [in] */ uint dwWin32ThreadId,
            /* [out] */ uint pstidThread);

        [PreserveSig]
        int GetScriptThreadState(
            /* [in] */ uint stidThread,
            /* [out] */ out ScriptThreadState pstsState);

        [PreserveSig]
        int InterruptScriptThread(
            /* [in] */ uint stidThread,
            /* [in] */ ref EXCEPINFO pexcepinfo,
            /* [in] */ uint dwFlags);

        [PreserveSig]
        int Clone(
            /* [out] */ out IActiveScript ppscript);
    }

}
