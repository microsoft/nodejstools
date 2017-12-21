// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Jade
{
    internal static class JadeClassificationTypes
    {
        public const string Filter = "JadeFilter";
        public const string Keyword = "JadeKeyword"; // break case do ...
        public const string Variable = "JadeVariable"; // #{foo}
        public const string ClassLiteral = "JadeClassLiteral"; // a.foo
        public const string IdLiteral = "JadeIdLiteral"; // a#foo
    }
}
