// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    internal class ProvideDebugLanguageAttribute : RegistrationAttribute
    {
        private readonly string _languageGuid, _languageName, _engineGuid, _eeGuid;

        public ProvideDebugLanguageAttribute(string languageName, string languageGuid, string eeGuid, string debugEngineGuid)
        {
            this._languageName = languageName;
            this._languageGuid = languageGuid;
            this._eeGuid = eeGuid;
            this._engineGuid = debugEngineGuid;
        }

        public override void Register(RegistrationContext context)
        {
            var langSvcKey = context.CreateKey("Languages\\Language Services\\" + this._languageName + "\\Debugger Languages\\" + this._languageGuid);
            langSvcKey.SetValue("", this._languageName);
            // 994... is the vendor ID (Microsoft)
            var eeKey = context.CreateKey("AD7Metrics\\ExpressionEvaluator\\" + this._languageGuid + "\\{994B45C4-E6E9-11D2-903F-00C04FA302A1}");
            eeKey.SetValue("Language", this._languageName);
            eeKey.SetValue("Name", this._languageName);
            eeKey.SetValue("CLSID", this._eeGuid);

            var engineKey = eeKey.CreateSubkey("Engine");
            engineKey.SetValue("0", this._engineGuid);
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
