// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class PkgFiles : PkgStringArray, IFiles
    {
        public PkgFiles(JObject package) : base(package, "files") { }
    }
}

