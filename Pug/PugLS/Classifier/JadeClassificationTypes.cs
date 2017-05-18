// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.Pug
{
    internal static class PugClassificationTypes
    {
        public const string Filter = "PugFilter";
        public const string Keyword = "PugKeyword"; // break case do ...
        public const string Variable = "PugVariable"; // #{foo}
        public const string ClassLiteral = "PugClassLiteral"; // a.foo
        public const string IdLiteral = "PugIdLiteral"; // a#foo
    }
}
