/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace Microsoft.NodejsTools.Editor.BraceCompletion {
    internal static class BraceKind {
        public static class Parentheses {
            public const char Open = '(';
            public const char Close = ')';
        }

        public static class SquareBrackets {
            public const char Open = '[';
            public const char Close = ']';
        }

        public static class CurlyBrackets {
            public const char Open = '{';
            public const char Close = '}';
        }

        public static class SingleQuotes {
            public const char Open = '\'';
            public const char Close = '\'';
        }

        public static class DoubleQuotes {
            public const char Open = '"';
            public const char Close = '"';
        }
    }
}