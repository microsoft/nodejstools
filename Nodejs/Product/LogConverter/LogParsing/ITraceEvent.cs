// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace NodeLogConverter.LogParsing
{
    [Guid("8CC97F40-9028-4FF3-9B62-7D1F79CA7BCB")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITraceEvent
    {
        void Clone(
            /* [retval][out] */ out ITraceEvent NewEvent);

        void GetUserContext(
            /* [retval][out] */ out IntPtr UserContext);

        void GetEventRecord(
            /* [retval][out] */ out IntPtr EventRecord);

        void SetPayload(
            /* [size_is][in] */ IntPtr Payload,
            /* [in] */ uint PayloadSize);

        void SetEventDescriptor(
            /* [in] */ IntPtr /*PCEVENT_DESCRIPTOR*/ EventDescriptor);

        void SetProcessId(
            /* [in] */ uint ProcessId);

        void SetProcessorIndex(
            /* [in] */ uint ProcessorIndex);

        void SetThreadId(
            /* [in] */ uint ThreadId);

        void SetThreadTimes(
            /* [in] */ uint KernelTime,
            /* [in] */ uint UserTime);

        void SetActivityId(
            /* [in] */ ref Guid ActivityId);

        void SetTimeStamp(
            /* [in] */ ref long TimeStamp);

        void SetProviderId(
            /* [in] */ ref Guid ProviderId);
    }
}

