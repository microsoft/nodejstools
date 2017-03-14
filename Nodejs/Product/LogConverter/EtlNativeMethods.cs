// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.LogGeneration
{
    internal class EtlNativeMethods
    {
        [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint CloseTrace(ulong handle);

        [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern uint StartTraceW(out ulong sessionHandle, string sessionName, IntPtr properties);

        [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern uint ControlTraceW(ulong SessionHandle, string SessionName, IntPtr Properties, TraceControlCode controlCode);

        [DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint TraceEvent(ulong SessionHandle, IntPtr EventTrace);

        public const uint WNODE_FLAG_TRACED_GUID = 0x00020000;
        public const uint WNODE_FLAG_USE_TIMESTAMP = 0x00000200;
        public const uint PROCESS_TRACE_MODE_EVENT_RECORD = 0x10000000;

        internal static Guid CLSID_TraceRelogger = new Guid("7b40792d-05ff-44c4-9058-f440c71f17d4");
    }

    internal enum TraceControlCode
    {
        Query,
        Stop,
        Update,
        Flush
    }

    // http://msdn.microsoft.com/en-us/library/dd264810.aspx
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct MethodLoadUnload
    {
        public ulong MethodID;
        public ulong ModuleID;
        public ulong MethodStartAddress;
        public uint MethodSize;
        public uint MethodToken;
        public uint MethodFlags;
        //public uint X, Y, Z;
        //public uint Y;
    }

    // http://msdn.microsoft.com/en-us/library/vstudio/ff356159(v=vs.110).aspx
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ManagedModuleLoadUnload
    {
        public ulong ModuleID;
        public ulong AssemblyID;
        public uint ModuleFlags;
        public uint Reserved1;
        // ModuleILPath
        // ModuleNativePath
        // ClrInstanceID    [int 16]
        // ManagedPdbSignature  [Guid]
        // ManagedPdbAge        [int32]
        // ManagedPdbBuildPath  [string]
        // NativePdbSignature   [Guid]
        // NativePdgAge         [uint32]
        // NativePdbBuildPath   [string]
    }

    // http://msdn.microsoft.com/en-us/library/windows/desktop/aa364068(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ImageLoad
    {
        public uint ImageBase;
        public uint ImageSize;
        public uint ProcessId;
        public uint ImageChecksum;
        public uint TimeDateStamp;
        public uint Reserved0;
        public uint DefaultBase;
        public uint Reserved1;
        public uint Reserved2;
        public uint Reserved3;
        public uint Reserved4;
        //string FileName;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StackWalk
    {
        public ulong TimeStamp;
        public uint Process;
        public uint Thread;
    }

    internal struct ThreadInfo
    {
        public uint ProcessId;
        public uint TThreadId;
        public uint StackBase;
        public uint StackLimit;
        public uint UserStackBase;
        public uint UserStackLimit;
        public uint Affinity;
        public uint Win32StartAddr;
        public uint TebBase;
        public uint SubProcessTag;
        public byte BasePriority;
        public byte PagePriority;
        public byte IoPriority;
        public byte ThreadFlags;
    };

    // http://msdn.microsoft.com/en-us/library/windows/desktop/aa964789(v=vs.85).aspx
    internal struct ProcessInfo
    {
        public uint UniqueProcessKey;
        public uint ProcessId;
        public uint ParentId;
        public uint SessionId;
        public uint ExitStatus;
        public uint UserSID;               // we'll write 0 here indicating no sid
    };

    internal struct SampledProfile
    {
        public uint InstructionPointer;
        public uint ThreadId;
        public uint Count;
    }

#pragma warning disable 649
#pragma warning disable 169

    internal struct EVENT_TRACE
    {
        public EVENT_TRACE_HEADER Header;
        public uint InstanceId;
        public uint ParentInstanceId;
        public Guid ParentGuid;
        public IntPtr MofData;
        public uint MofLength;
        public uint ClientContext_Or_ETW_BUFFER_CONTEXT;
    }

    internal struct EVENT_TRACE_HEADER
    {
        public ushort Size;
        public byte HeaderType;
        public byte MarkerFlags;
        public byte Type;
        public byte Level;
        public ushort Version;
        public uint ThreadId;
        public uint ProcessId;
        public long TimeStamp;
        public Guid Guid;
        public uint Flags;
        public uint UserTime;
    }

    internal struct SYSTEMTIME
    {
        private short wYear;
        private short wMonth;
        private short wDayOfWeek;
        private short wDay;
        private short wHour;
        private short wMinute;
        private short wSecond;
        private short wMilliseconds;

        public SYSTEMTIME(DateTime time)
        {
            wYear = (short)time.Year;
            wMonth = (short)time.Month;
            wDayOfWeek = (short)time.DayOfWeek;
            wDay = (short)time.Day;
            wHour = (short)time.Hour;
            wMinute = (short)time.Minute;
            wSecond = (short)time.Second;
            wMilliseconds = (short)time.Millisecond;
        }

        public DateTime ToDateTime()
        {
            return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}:{4}:{5}.{6}", wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
            //return ToDateTime().ToString();
        }
    }

    internal struct EVENT_TRACE_PROPERTIES
    {
        public WNODE_HEADER Wnode;
        public uint BufferSize;
        public uint MinimumBuffers;
        public uint MaximumBuffers;
        public uint MaximumFileSize;
        public uint LogFileMode;
        public uint FlushTimer;
        public uint EnableFlags;
        public int AgeLimit;
        public uint NumberOfBuffers;
        public uint FreeBuffers;
        public uint EventsLost;
        public uint BuffersWritten;
        public uint LogBuffersLost;
        public uint RealTimeBuffersLost;
        public IntPtr LoggerThreadId;
        public uint LogFileNameOffset;
        public uint LoggerNameOffset;
    }

    internal struct WNODE_HEADER
    {
        public uint BufferSize;
        private uint ProviderId;
        private ulong HistoricalContext;
        public ulong TimeStamp;
        public Guid Guid;
        public uint ClientContext;
        public uint Flags;
    }

    internal struct EVENT_RECORD
    {
        public EVENT_HEADER EventHeader;
        public ETW_BUFFER_CONTEXT BufferContext;
        public ushort ExtendedDataCount;
        public ushort UserDataLength;
        public IntPtr /*EVENT_HEADER_EXTENDED_DATA_ITEM*/ ExtendedData;
        public IntPtr UserData;
        public IntPtr UserContext;
    }

    internal struct EVENT_HEADER
    {
        public ushort Size;
        public ushort HeaderType;
        public ushort Flags;
        public ushort EventProperty;
        public uint ThreadId;
        public uint ProcessId;
        public long TimeStamp;
        public Guid ProviderId;
        public EVENT_DESCRIPTOR EventDescriptor;
        public ulong ProcessorTime;
        public Guid ActivityId;
    }

    internal struct EVENT_DESCRIPTOR
    {
        public ushort Id;
        public byte Version;
        public byte Channel;
        public byte Level;
        public byte Opcode;
        public ushort Task;
        public ulong Keyword;
    }

    internal struct EVENT_DATA_DESCRIPTOR
    {
        public ulong Ptr;
        public uint Size;
        public uint Reserved;
    }

    internal struct ETW_BUFFER_CONTEXT
    {
        private byte ProcessorNumber;
        private byte Alignment;
        private ushort LoggerId;
    }

    internal struct EVENT_HEADER_EXTENDED_DATA_ITEM
    {
        private ushort Reserved1;
        private ushort ExtType;
        private ushort Linkage;
        private ushort DataSize;
        private ulong DataPtr;
    }

    internal struct TRACE_GUID_REGISTRATION
    {
        public IntPtr Guid;
        public IntPtr RegHandle;
    }

    // http://msdn.microsoft.com/en-us/library/aa363748(v=vs.85).aspx
    [StructLayout(LayoutKind.Explicit)]
    internal struct EventTrace_Header
    {
        [FieldOffset(0)]
        public uint BufferSize;
        [FieldOffset(4)]
        public uint Version;
        [FieldOffset(8)]
        public uint ProviderVersion;
        [FieldOffset(12)]
        public uint NumberOfProcessors;
        [FieldOffset(16)]
        public ulong EndTime;
        [FieldOffset(24)]
        public uint TimerResolution;
        [FieldOffset(28)]
        public uint MaxFileSize;
        [FieldOffset(32)]
        public uint LogFileMode;
        [FieldOffset(36)]
        public uint BuffersWritten;
        [FieldOffset(40)]
        public uint StartBuffers;
        [FieldOffset(44)]
        public uint PointerSize;
        [FieldOffset(48)]
        public uint EventsLost;
        [FieldOffset(52)]
        public uint CPUSpeed;
        [FieldOffset(56)]
        public uint LoggerName;
        [FieldOffset(60)]
        public uint LogFileName;
        [FieldOffset(64)]
        public IntPtr TimeZoneInformation;
        [FieldOffset(240)]
        public ulong BootTime;
        [FieldOffset(248)]
        public ulong PerfFreq;
        [FieldOffset(256)]
        public ulong StartTime;
        [FieldOffset(264)]
        public uint ReservedFlags;
        [FieldOffset(268)]
        public uint BuffersLost;
    }
}

