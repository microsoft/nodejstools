// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools
{
    internal static class PkgCmdId
    {
        public const int cmdidSetAsNodejsStartupFile = 0x203;

        public const int cmdidImportWizard = 0x205;
        public const int cmdidJspsProjectMigrate = 0x206;
        public const int cmdidJspsProjectRevert = 0x207;

        public const int cmdidDiagnostics = 0x208;
        public const int cmdidAddFileCommand = 0x211;

        public const int cmdidNpmManageModules = 0x300;
        public const int cmdidNpmInstallModules = 0x301;
        public const int cmdidNpmUpdateModules = 0x302;
        public const int cmdidNpmUninstallModule = 0x303;
        public const int cmdidNpmInstallSingleMissingModule = 0x304;
        public const int cmdidNpmUpdateSingleModule = 0x305;
        public const int cmdidNpmOpenModuleHomepage = 0x306;
        public const int menuIdNpm = 0x3000;

        public const int cmdidWorkSpaceNpmInstallMissing = 0x0200;
        public const int cmdidWorkSpaceNpmInstallNew = 0x0201;
        public const int cmdidWorkSpaceNpmUpdate = 0x0202;
        public const int cmdidWorkSpaceNpmDynamicScript = 0x1000;

        // only allow 100 scripts
        public const int cmdidWorkSpaceNpmDynamicScriptMax = cmdidWorkSpaceNpmDynamicScript + 100;

        // Workspace cmdIds
        public const int cmdid_DebugActionContext = 0x1000;

        public const int cmdid_BuildActionContext = 0x1000;
        public const int cmdid_RebuildActionContext = 0x1010;
        public const int cmdid_CleanActionContext = 0x1020;
    }
}
