// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    public static class CommandTargetExtensions
    {
        public static void Type(this IOleCommandTarget target, string text)
        {
            var guid = VSConstants.VSStd2K;
            var variantMem = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(VARIANT)));
            try
            {
                for (int i = 0; i < text.Length; i++)
                {
                    switch (text[i])
                    {
                        case '\r': target.Enter(); break;
                        case '\t': target.Tab(); break;
                        default:
                            Marshal.GetNativeVariantForObject((ushort)text[i], variantMem);
                            target.Exec(
                                ref guid,
                                (int)VSConstants.VSStd2KCmdID.TYPECHAR,
                                0,
                                variantMem,
                                IntPtr.Zero
                            );
                            break;
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(variantMem);
            }
        }

        public static void Enter(this IOleCommandTarget target)
        {
            var guid = VSConstants.VSStd2K;
            target.Exec(ref guid, (int)VSConstants.VSStd2KCmdID.RETURN, 0, IntPtr.Zero, IntPtr.Zero);
        }

        public static void Tab(this IOleCommandTarget target)
        {
            var guid = VSConstants.VSStd2K;
            target.Exec(ref guid, (int)VSConstants.VSStd2KCmdID.TAB, 0, IntPtr.Zero, IntPtr.Zero);
        }

        public static void Backspace(this IOleCommandTarget target)
        {
            var guid = VSConstants.VSStd2K;
            target.Exec(ref guid, (int)VSConstants.VSStd2KCmdID.BACKSPACE, 0, IntPtr.Zero, IntPtr.Zero);
        }

        public static void MemberList(this IOleCommandTarget target)
        {
            var guid = VSConstants.VSStd2K;
            ErrorHandler.ThrowOnFailure(target.Exec(ref guid, (int)VSConstants.VSStd2KCmdID.SHOWMEMBERLIST, 0, IntPtr.Zero, IntPtr.Zero));
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct VARIANT
        {
            [FieldOffset(0)]
            public ushort vt;
            [FieldOffset(8)]
            public IntPtr pdispVal;
            [FieldOffset(8)]
            public byte ui1;
            [FieldOffset(8)]
            public ushort ui2;
            [FieldOffset(8)]
            public uint ui4;
            [FieldOffset(8)]
            public ulong ui8;
            [FieldOffset(8)]
            public float r4;
            [FieldOffset(8)]
            public double r8;
        }
    }
}

