// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    [ComImport]
    [Guid("78A67F33-22CF-426C-8C90-B6E18FD35E0F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    public interface SVsFeatureFlags { }

    [ComImport]
    [Guid("AD44B8B9-B646-4B18-8847-150695AEC480")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    public interface IVsFeatureFlags
    {
        bool IsFeatureEnabled(string name, bool defaultValue);
    }
}
