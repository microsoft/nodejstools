// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudioTools.Project
{
    internal static class UnsafeNativeMethods
    {
        [DllImport(ExternDll.Kernel32, EntryPoint = "GlobalLock", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GlobalLock(IntPtr h);

        [DllImport(ExternDll.Kernel32, EntryPoint = "GlobalUnlock", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool GlobalUnLock(IntPtr h);

        [DllImport(ExternDll.Kernel32, EntryPoint = "GlobalSize", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GlobalSize(IntPtr h);

        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int OleSetClipboard(Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject);

        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int OleGetClipboard(out Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject);

        [DllImport(ExternDll.Ole32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int OleFlushClipboard();

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int OpenClipboard(IntPtr newOwner);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int EmptyClipboard();

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int CloseClipboard();

        [DllImport(ExternDll.Shell32, EntryPoint = "DragQueryFileW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern uint DragQueryFile(IntPtr hDrop, uint iFile, char[] lpszFile, uint cch);

        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern uint RegisterClipboardFormat(string format);
    }
}
