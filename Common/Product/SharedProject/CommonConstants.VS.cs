// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudioTools
{
    internal static partial class CommonConstants
    {
        //"Open Folder in Windows Explorer" command ID.
        //Don't change this! This is Visual Studio constant.
        public const VsCommands2K OpenFolderInExplorerCmdId = (VsCommands2K)1635;

        //These are VS internal constants - don't change them
        public static Guid Std97CmdGroupGuid = typeof(VSConstants.VSStd97CmdID).GUID;
        public static Guid Std2KCmdGroupGuid = typeof(VSConstants.VSStd2KCmdID).GUID;

        //Command statuses
        public const int NotSupportedInvisibleCmdStatus = (int)OleConstants.OLECMDERR_E_NOTSUPPORTED |
                            (int)OleConstants.OLECMDSTATE_INVISIBLE;
        public const int SupportedEnabledCmdStatus = (int)(OLECMDF.OLECMDF_SUPPORTED |
                            OLECMDF.OLECMDF_ENABLED);
        public const int SupportedCmdStatus = (int)OLECMDF.OLECMDF_SUPPORTED;
    }
}
