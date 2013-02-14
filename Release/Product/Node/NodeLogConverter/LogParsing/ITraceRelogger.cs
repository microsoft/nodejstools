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
    [Guid("F754AD43-3BCC-4286-8009-9C5DA214E84E")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITraceRelogger {
        void AddLogfileTraceStream(
            [MarshalAs(UnmanagedType.BStr)]
            /* [in] */ string LogfileName,
            /* [in] */ IntPtr UserContext,
            /* [retval][out] */ out ulong TraceHandle);

        void AddRealtimeTraceStream(
            [MarshalAs(UnmanagedType.BStr)]
            /* [in] */ string LoggerName,
            /* [in] */ IntPtr UserContext,
            /* [retval][out] */ out ulong TraceHandle);

        void RegisterCallback(
            /* [in] */ ITraceEventCallback Callback);

        void Inject(

            /* [in] */ ITraceEvent Event);

        void CreateEventInstance(
            /* [in] */ ulong TraceHandle,
            /* [in] */ uint Flags,
            /* [retval][out] */ out ITraceEvent Event);

        void ProcessTrace();

        void SetOutputFilename(
            [MarshalAs(UnmanagedType.BStr)]
            /* [in] */ string LogfileName);

        void SetCompressionMode(
            [MarshalAs(UnmanagedType.Bool)]
            /* [in] */ bool CompressionMode);

        void Cancel();
    }
}
