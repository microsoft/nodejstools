// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Options
{
    internal class NodejsDiagnosticsOptionsPage : NodejsDialogPage
    {
        private bool _isLiveDiagnosticsEnabled;
        private const string IsLiveDiagnosticsEnabledSetting = "IsLiveDiagnosticsEnabled";

        public NodejsDiagnosticsOptionsPage() : base("Diagnostics")
        {
            this._isLiveDiagnosticsEnabled = !NodejsPackage.Instance.Zombied && (LoadBool(IsLiveDiagnosticsEnabledSetting) ?? false);
        }

        public bool IsLiveDiagnosticsEnabled
        {
            get
            {
                return !NodejsPackage.Instance.Zombied && this._isLiveDiagnosticsEnabled;
            }
            set
            {
                this._isLiveDiagnosticsEnabled = value;
                SaveBool(IsLiveDiagnosticsEnabledSetting, value);
            }
        }
    }
}

