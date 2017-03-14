// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace NodeLogConverter
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
           IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
           uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport("dbghelp.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymInitialize(IntPtr hProcess, IntPtr UserSearchPath, bool fInvadeProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        internal static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, [MarshalAs(UnmanagedType.LPStr)] string ImageName,
            [MarshalAs(UnmanagedType.LPStr)]
          string ModuleName,
          ulong BaseOfDll,
          uint DllSize,
          IntPtr Data,
          uint Flags
        );

        [DllImport("dbghelp.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymGetModuleInfo64(IntPtr hProcess, ulong dwAddr, ref IMAGEHLP_MODULE64 ModuleInfo);

        [DllImport("dbghelp.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymCleanup(IntPtr hProcess);
    }

    [Flags]
    public enum DuplicateOptions : uint
    {
        DUPLICATE_CLOSE_SOURCE = 0x00000001,
        DUPLICATE_SAME_ACCESS = 0x00000002
    }

    internal struct IMAGEHLP_MODULE64
    {
        public uint SizeOfStruct;
        public ulong BaseOfImage;
        public uint ImageSize;
        public uint TimeDateStamp;
        public uint CheckSum;
        public uint NumSyms;
        public int SymType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ModuleName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ImageName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string LoadedImageName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string LoadedPdbName;
        public uint CVSig;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260 * 3)]
        public string CVData;
        public uint PdbSig;
        public Guid PdbSig70;
        public uint PdbAge;
        public bool PdbUnmatched;
        public bool DbgUnmatched;
        public bool LineNumbers;
        public bool GlobalSymbols;
        public bool TypeInfo;
        public bool SourceIndexed;
        public bool Publics;
    }
}

