// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace NodeLogConverter.LogParsing
{
    [Guid("3ED25501-593F-43E9-8F38-3AB46F5A4A52")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITraceEventCallback
    {
        void OnBeginProcessTrace(
            /* [in] */ ITraceEvent HeaderEvent,
            /* [in] */ ITraceRelogger Relogger);

        void OnFinalizeProcessTrace(
            /* [in] */ ITraceRelogger Relogger);

        void OnEvent( /* [in] */ ITraceEvent Event, /* [in] */ ITraceRelogger Relogger);
    }
}

