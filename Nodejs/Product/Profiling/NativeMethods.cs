// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools
{
    internal static class NativeMethods
    {
        [DllImport("kernel32", EntryPoint = "GetBinaryTypeW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        private static extern bool _GetBinaryType(string lpApplicationName, out GetBinaryTypeResult lpBinaryType);

        private enum GetBinaryTypeResult : uint
        {
            SCS_32BIT_BINARY = 0,
            SCS_DOS_BINARY = 1,
            SCS_WOW_BINARY = 2,
            SCS_PIF_BINARY = 3,
            SCS_POSIX_BINARY = 4,
            SCS_OS216_BINARY = 5,
            SCS_64BIT_BINARY = 6
        }

        public static ProcessorArchitecture GetBinaryType(string path)
        {
            GetBinaryTypeResult result;

            if (_GetBinaryType(path, out result))
            {
                switch (result)
                {
                    case GetBinaryTypeResult.SCS_32BIT_BINARY:
                        return ProcessorArchitecture.X86;
                    case GetBinaryTypeResult.SCS_64BIT_BINARY:
                        return ProcessorArchitecture.Amd64;
                    case GetBinaryTypeResult.SCS_DOS_BINARY:
                    case GetBinaryTypeResult.SCS_WOW_BINARY:
                    case GetBinaryTypeResult.SCS_PIF_BINARY:
                    case GetBinaryTypeResult.SCS_POSIX_BINARY:
                    case GetBinaryTypeResult.SCS_OS216_BINARY:
                    default:
                        break;
                }
            }

            return ProcessorArchitecture.None;
        }
    }
}

