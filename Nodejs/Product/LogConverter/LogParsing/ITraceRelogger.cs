// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace NodeLogConverter.LogParsing
{
    [Guid("F754AD43-3BCC-4286-8009-9C5DA214E84E")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITraceRelogger
    {
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

