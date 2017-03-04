// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudioTools
{
    public abstract class ClipboardServiceBase
    {
        public abstract void SetClipboard(IDataObject dataObject);

        public abstract IDataObject GetClipboard();

        public abstract void FlushClipboard();

        public abstract bool OpenClipboard();

        public abstract void EmptyClipboard();

        public abstract void CloseClipboard();
    }
}

