// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.TypeScript
{
    internal sealed class TsConfigJson
    {
        public readonly bool NoEmit;

        public readonly string OutFile;

        public readonly string OutDir;

        public readonly string RootDir;

        public TsConfigJson(dynamic package)
        {
            var compilerOptions = package["compilerOptions"] as JObject;

            this.NoEmit = NoEmitSet(compilerOptions);
            this.OutFile = compilerOptions["outFile"]?.ToString();
            this.OutDir = compilerOptions["outDir"]?.ToString();
            this.RootDir = compilerOptions["rootDir"]?.ToString();
        }

        private static bool NoEmitSet(JObject compilerOptions)
        {
            return bool.TryParse(compilerOptions["noEmit"]?.ToString(), out var result) && result;
        }
    }
}
