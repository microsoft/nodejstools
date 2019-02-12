// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.VisualStudioTools.Project
{
    internal static class NativeMethods
    {
        // IIDS
        public static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");

        public const int WM_INITDIALOG = 0x0110;
        public const int WM_SIZE = 0x0005;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_CONTROLPARENT = 0x00010000;
        public const int OLECMDERR_E_NOTSUPPORTED = unchecked((int)0x80040100);
        public const int E_FAIL = unchecked((int)0x80004005);

        public const int
            IDOK = 1,
            IDCANCEL = 2,
            IDABORT = 3,
            IDRETRY = 4,
            IDIGNORE = 5,
            IDYES = 6,
            IDNO = 7,
            IDCLOSE = 8,
            IDHELP = 9,
            IDTRYAGAIN = 10,
            IDCONTINUE = 11;

        /// <devdoc>
        /// Helper class for setting the text parameters to OLECMDTEXT structures.
        /// </devdoc>
        public static class OLECMDTEXT
        {
            public static void SetText(IntPtr pCmdTextInt, string text)
            {
                var pCmdText = (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));
                var menuText = text.ToCharArray();

                // Get the offset to the rgsz param.  This is where we will stuff our text
                //
                var offset = Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "rgwz");
                var offsetToCwActual = Marshal.OffsetOf(typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT), "cwActual");

                // The max chars we copy is our string, or one less than the buffer size,
                // since we need a null at the end.
                //
                var maxChars = Math.Min((int)pCmdText.cwBuf - 1, menuText.Length);

                Marshal.Copy(menuText, 0, (IntPtr)((long)pCmdTextInt + (long)offset), maxChars);

                // append a null character
                Marshal.WriteInt16((IntPtr)((long)pCmdTextInt + (long)offset + maxChars * 2), 0);

                // write out the length
                // +1 for the null char
                Marshal.WriteInt32((IntPtr)((long)pCmdTextInt + (long)offsetToCwActual), maxChars + 1);
            }

            /// <summary>
            /// Gets the flags of the OLECMDTEXT structure
            /// </summary>
            /// <param name="pCmdTextInt">The structure to read.</param>
            /// <returns>The value of the flags.</returns>
            public static OLECMDTEXTF GetFlags(IntPtr pCmdTextInt)
            {
                var pCmdText = (Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT)Marshal.PtrToStructure(pCmdTextInt, typeof(Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT));

                if ((pCmdText.cmdtextf & (int)OLECMDTEXTF.OLECMDTEXTF_NAME) != 0)
                {
                    return OLECMDTEXTF.OLECMDTEXTF_NAME;
                }

                if ((pCmdText.cmdtextf & (int)OLECMDTEXTF.OLECMDTEXTF_STATUS) != 0)
                {
                    return OLECMDTEXTF.OLECMDTEXTF_STATUS;
                }

                return OLECMDTEXTF.OLECMDTEXTF_NONE;
            }

            /// <summary>
            /// Flags for the OLE command text
            /// </summary>
            public enum OLECMDTEXTF
            {
                /// <summary>No flag</summary>
                OLECMDTEXTF_NONE = 0,
                /// <summary>The name of the command is required.</summary>
                OLECMDTEXTF_NAME = 1,
                /// <summary>A description of the status is required.</summary>
                OLECMDTEXTF_STATUS = 2
            }
        }

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern bool GetClientRect(IntPtr hWnd, out User32RECT lpRect);

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void SetErrorDescription(string description, params object[] args)
        {
            ErrorHandler.ThrowOnFailure(CreateErrorInfo(out var errInfo));

            errInfo.SetDescription(string.Format(description, args));
            var guidNull = Guid.Empty;
            errInfo.SetGUID(ref guidNull);
            errInfo.SetHelpFile(null);
            errInfo.SetHelpContext(0);
            errInfo.SetSource("");
            var errorInfo = errInfo as IErrorInfo;
            SetErrorInfo(0, errorInfo);
        }

        [DllImport("oleaut32")]
        private static extern int CreateErrorInfo(out ICreateErrorInfo errInfo);

        [DllImport("oleaut32")]
        private static extern int SetErrorInfo(uint dwReserved, IErrorInfo perrinfo);

        public const ushort CF_HDROP = 15; // winuser.h
        public const uint MK_CONTROL = 0x0008; //winuser.h
        public const uint MK_SHIFT = 0x0004;
        public const int MAX_PATH = 260; // windef.h	
        public const int MAX_FOLDER_PATH = MAX_PATH - 12;   // folders need to allow 8.3 filenames, so MAX_PATH - 12

        // APIS

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);

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

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetFinalPathNameByHandle(
            SafeHandle hFile,
            [Out]StringBuilder lpszFilePath,
            uint cchFilePath,
            uint dwFlags
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            FileDesiredAccess dwDesiredAccess,
            FileShareFlags dwShareMode,
            IntPtr lpSecurityAttributes,
            FileCreationDisposition dwCreationDisposition,
            FileFlagsAndAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [Flags]
        public enum FileDesiredAccess : uint
        {
            FILE_LIST_DIRECTORY = 1
        }

        [Flags]
        public enum FileShareFlags : uint
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        [Flags]
        public enum FileCreationDisposition : uint
        {
            OPEN_EXISTING = 3
        }

        [Flags]
        public enum FileFlagsAndAttributes : uint
        {
            FILE_FLAG_BACKUP_SEMANTICS = 0x02000000
        }

        [DllImport(ExternDll.Kernel32, EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetFinalPathNameByHandle(SafeFileHandle handle, [In, Out] StringBuilder path, int bufLen, int flags);

        [DllImport(ExternDll.Kernel32, EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr SecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        /// <summary>
        /// Given a directory, actual or symbolic, return the actual directory path.
        /// </summary>
        /// <param name="symlink">DirectoryInfo object for the suspected symlink.</param>
        /// <returns>A string of the actual path.</returns>
        internal static string GetAbsolutePathToDirectory(string symlink)
        {
            const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            const int DEVICE_QUERY_ACCESS = 0;

            using (var directoryHandle = CreateFile(
                symlink,
                DEVICE_QUERY_ACCESS,
                FileShare.Write,
                System.IntPtr.Zero,
                FileMode.Open,
                FILE_FLAG_BACKUP_SEMANTICS,
                System.IntPtr.Zero))
            {
                if (directoryHandle.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                var path = new StringBuilder(512);
                var pathSize = GetFinalPathNameByHandle(directoryHandle, path, path.Capacity, 0);
                if (pathSize < 0)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                // UNC Paths will start with \\?\.  Remove this if present as this isn't really expected on a path.
                var pathString = path.ToString();
                return pathString.StartsWith(@"\\?\") ? pathString.Substring(4) : pathString;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool MoveFile(string src, string dst);
    }

    internal struct User32RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int Width => this.right - this.left;

        public int Height => this.bottom - this.top;
    }

    [Guid("22F03340-547D-101B-8E65-08002B2BD119")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICreateErrorInfo
    {
        int SetGUID(
             ref Guid rguid
         );

        int SetSource(string szSource);

        int SetDescription(string szDescription);

        int SetHelpFile(string szHelpFile);

        int SetHelpContext(uint dwHelpContext);
    }
}
