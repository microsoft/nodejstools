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

namespace NodeLogConverter.LogParsing {
    [Guid("3ED25501-593F-43E9-8F38-3AB46F5A4A52")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITraceEventCallback {

        void OnBeginProcessTrace(
            /* [in] */ ITraceEvent HeaderEvent,
            /* [in] */ ITraceRelogger Relogger);

        void OnFinalizeProcessTrace(
            /* [in] */ ITraceRelogger Relogger);

        void OnEvent( /* [in] */ ITraceEvent Event, /* [in] */ ITraceRelogger Relogger);
    }
}
