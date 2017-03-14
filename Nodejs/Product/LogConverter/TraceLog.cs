// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.LogGeneration
{
    internal class TraceLog
    {
        private readonly ulong _handle;
        private readonly string _sessionName;
        private IntPtr _traceProps;

        public static readonly Guid ThreadEventGuid = new Guid("{3D6FA8D1-FE05-11D0-9DDA-00C04FD7BA7C}");
        public static readonly Guid ProcessEventGuid = new Guid("{3D6FA8D0-FE05-11D0-9DDA-00C04FD7BA7C}");
        public static readonly Guid StackWalkEventGuid = new Guid("{DEF2FE46-7BD6-4B80-BD94-F57FE20D0CE3}");
        public static readonly Guid PerfInfoEventGuid = new Guid("{CE1DBFB4-137E-4DA6-87B0-3F59AA102CBC}");
        public static readonly Guid MethodEventGuid = new Guid("{3044F61A-99B0-4C21-B203-D39423C73B00}");
        public static readonly Guid LoaderEventGuid = new Guid("{d00792da-07b7-40f5-97eb-5d974e054740}");
        public static readonly Guid ImageEventGuid = new Guid("{2cb15d1d-5fc1-11d2-abe1-00a0c911f518}");
        public static readonly Guid ClrRuntimeProvider = new Guid("{e13c0d23-ccbc-4e12-931b-d9cc2eee27e4}");

        internal const int EVENT_TRACE_PRIVATE_IN_PROC = 0x20000;
        internal const int EVENT_TRACE_PRIVATE_LOGGER_MODE = 0x800;
        internal const int EVENT_TRACE_FILE_MODE_SEQUENTIAL = 0x0001;

        private TraceLog(ulong handle, string sessionName, IntPtr props)
        {
            _handle = handle;
            _sessionName = sessionName;
            _traceProps = props;
        }

        public static void WriteEvent()
        {
        }

        ~TraceLog()
        {
            if (_traceProps != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_traceProps);
            }
        }

        public void Stop()
        {
            var status = EtlNativeMethods.ControlTraceW(
                _handle,
                _sessionName,
                _traceProps,
                TraceControlCode.Stop
            );
            if (status != 0 && status != 234 /*ERROR_MORE_DATA*/)
            {
                throw new Win32Exception((int)status);
            }
            GC.KeepAlive(this);
        }

        /// <summary>
        /// Starts a new trace log file with the given session name writing it to the specified file.
        /// 
        /// If a session name is not provided a unique name will be generated
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static TraceLog Start(string filename, string sessionName = null)
        {
            if (sessionName == null)
            {
                sessionName = Guid.NewGuid().ToString();
            }
            var bufferSize = Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES)) +
                (sessionName.Length + 1) * 2 +
                (filename.Length + 1) * 2;

            var mem = Marshal.AllocHGlobal(bufferSize);
            for (int i = 0; i < bufferSize; i++)
            {
                Marshal.WriteByte(mem, i, 0);
            }

            if (mem == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            EVENT_TRACE_PROPERTIES eventProps = new EVENT_TRACE_PROPERTIES();
            eventProps.Wnode.BufferSize = (uint)bufferSize;
            eventProps.BufferSize = 512;
            eventProps.Wnode.Guid = Guid.NewGuid();
            eventProps.LogFileMode = EVENT_TRACE_PRIVATE_IN_PROC | EVENT_TRACE_PRIVATE_LOGGER_MODE | EVENT_TRACE_FILE_MODE_SEQUENTIAL;
            eventProps.MaximumFileSize = 0;
            eventProps.Wnode.Flags = EtlNativeMethods.WNODE_FLAG_TRACED_GUID;
            eventProps.Wnode.TimeStamp = 0;
            eventProps.Wnode.ClientContext = 0;
            eventProps.LoggerNameOffset = (uint)Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES));
            eventProps.LogFileNameOffset = (uint)(Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES)) + (sessionName.Length + 1) * 2);

            Marshal.StructureToPtr(eventProps, mem, false);

            int offset = (int)eventProps.LogFileNameOffset;
            for (int i = 0; i < filename.Length; i++, offset += 2)
            {
                Marshal.WriteInt16(mem, offset, filename[i]);
            }
            Marshal.WriteInt16(mem, offset, 0);

            bool success = false;
            try
            {
                ulong handle;
                var error = EtlNativeMethods.StartTraceW(
                    out handle,
                    sessionName,
                    mem
                );

                if (error != 0)
                {
                    throw new Win32Exception((int)error);
                }

                var res = new TraceLog(handle, sessionName, mem);
                success = true;
                return res;
            }
            finally
            {
                if (!success)
                {
                    Marshal.FreeHGlobal(mem);
                }
            }
        }

        /// <summary>
        /// Writes out trace data.  data should be a series of structures, starting with a 
        /// EVENT_TRACE_HEADER structure.
        /// </summary>
        /// <param name="data"></param>
        public void Trace(EVENT_TRACE_HEADER header, params object[] data)
        {
            IntPtr traceData;
            int size = Marshal.SizeOf(typeof(EVENT_TRACE_HEADER));

            for (int i = 0; i < data.Length; i++)
            {
                string strData;
                byte[] asciiStrData;

                if ((strData = data[i] as string) != null)
                {
                    size += strData.Length * 2 + 2;
                }
                else if ((asciiStrData = data[i] as byte[]) != null)
                {
                    size += asciiStrData.Length + 1;
                }
                else
                {
                    size += Marshal.SizeOf(data[i]);
                }
            }

            IntPtr tempData = traceData = Marshal.AllocHGlobal(size);
            try
            {
                if (traceData == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }

                tempData = new IntPtr(tempData.ToInt64() + Marshal.SizeOf(typeof(EVENT_TRACE_HEADER)));
                for (int i = 0; i < data.Length; i++)
                {
                    string strData;
                    byte[] asciiStrData;

                    if ((strData = data[i] as string) != null)
                    {
                        for (int j = 0; j < strData.Length; j++)
                        {
                            Marshal.WriteInt16(tempData, j * 2, strData[j]);
                        }
                        Marshal.WriteInt16(tempData, strData.Length * 2, 0);
                        tempData = new IntPtr(tempData.ToInt64() + strData.Length * 2 + 2);
                    }
                    else if ((asciiStrData = data[i] as byte[]) != null)
                    {
                        for (int j = 0; j < asciiStrData.Length; j++)
                        {
                            Marshal.WriteByte(tempData, j, asciiStrData[j]);
                        }
                        Marshal.WriteByte(tempData, asciiStrData.Length, 0);
                        tempData = new IntPtr(tempData.ToInt64() + asciiStrData.Length + 1);
                    }
                    else
                    {
                        Marshal.StructureToPtr(data[i], tempData, false);
                        tempData = new IntPtr(tempData.ToInt64() + Marshal.SizeOf(data[i]));
                    }
                }

                header.Size = (ushort)size;
                Marshal.StructureToPtr(header, traceData, false);

                var res = EtlNativeMethods.TraceEvent(_handle, traceData);
                if (res != 0)
                {
                    throw new Win32Exception((int)res);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(traceData);
            }
        }
    }
}

