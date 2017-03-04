// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.SourceMapping
{
    internal class JavaScriptSourceMapInfo
    {
        internal readonly string[] Lines;
        internal readonly SourceMap Map;

        internal JavaScriptSourceMapInfo(SourceMap map, string[] lines)
        {
            this.Map = map;
            this.Lines = lines;
        }
    }
}

