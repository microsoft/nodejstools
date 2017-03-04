// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools
{
    internal class ClipboardService : ClipboardServiceBase
    {
        public override void SetClipboard(IDataObject dataObject)
        {
            ErrorHandler.ThrowOnFailure(UnsafeNativeMethods.OleSetClipboard(dataObject));
        }

        public override IDataObject GetClipboard()
        {
            IDataObject res;
            ErrorHandler.ThrowOnFailure(UnsafeNativeMethods.OleGetClipboard(out res));
            return res;
        }

        public override void FlushClipboard()
        {
            ErrorHandler.ThrowOnFailure(UnsafeNativeMethods.OleFlushClipboard());
        }

        public override bool OpenClipboard()
        {
            var res = UnsafeNativeMethods.OpenClipboard(IntPtr.Zero);
            ErrorHandler.ThrowOnFailure(res);
            return res == 1;
        }

        public override void EmptyClipboard()
        {
            ErrorHandler.ThrowOnFailure(UnsafeNativeMethods.EmptyClipboard());
        }

        public override void CloseClipboard()
        {
            ErrorHandler.ThrowOnFailure(UnsafeNativeMethods.CloseClipboard());
        }
    }
}

