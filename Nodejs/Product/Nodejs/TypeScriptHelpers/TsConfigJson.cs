// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.TypeScript
{
    internal class TsConfigJson
    {
        public readonly bool NoEmit;

        public readonly string OutFile;

        public readonly string OutDir;

        public readonly string RootDir;

        public TsConfigJson(dynamic package)
        {
            var compilerOptions = package["compilerOptions"] as JObject;

            this.NoEmit = this.SetNoEmit(compilerOptions);
            this.OutFile = compilerOptions["outFile"]?.ToString();
            this.OutDir = compilerOptions["outDir"]?.ToString();
            this.RootDir = compilerOptions["rootDir"]?.ToString();
        }

        private bool SetNoEmit(JObject compilerOptions)
        {
            return bool.TryParse(compilerOptions["noEmit"]?.ToString(), out var result) && result;
        }

        public static bool IsValidFile(string filePath)
        {
            return filePath?.EndsWith("tsconfig.json", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
