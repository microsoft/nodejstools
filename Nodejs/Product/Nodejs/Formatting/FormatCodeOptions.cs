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

using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Formatting {
    [ComVisible(true)]
    public class FormatCodeOptions {
        public int IndentSize = 4;
        public int TabSize = 4;
        public string NewLineCharacter = "\r\n";
        public bool ConvertTabsToSpaces;

        public bool InsertSpaceAfterCommaDelimiter;
        public bool InsertSpaceAfterSemicolonInForStatements;
        public bool InsertSpaceBeforeAndAfterBinaryOperators;
        public bool InsertSpaceAfterKeywordsInControlFlowStatements;
        public bool InsertSpaceAfterFunctionKeywordForAnonymousFunctions;
        public bool InsertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis;
        public bool PlaceOpenBraceOnNewLineForFunctions;
        public bool PlaceOpenBraceOnNewLineForControlBlocks;
    }
}
