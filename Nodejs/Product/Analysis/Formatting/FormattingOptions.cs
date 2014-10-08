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

namespace Microsoft.NodejsTools.Formatting {

    internal sealed class FormattingOptions {
        public int? SpacesPerIndent { get; set; }
        public string NewLine { get; set; }
        public bool OpenBracesOnNewLineForControl { get; set; }
        public bool OpenBracesOnNewLineForFunctions { get; set; }
        public bool SpaceAfterComma { get; set; }
        public bool SpaceAfterSemiColonInFor { get; set; }
        public bool SpaceBeforeAndAfterBinaryOperator { get; set; }
        public bool SpaceAfterKeywordsInControlFlowStatements { get; set; }
        public bool SpaceAfterFunctionInAnonymousFunctions { get; set; }
        public bool SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis { get; set; }

        public FormattingOptions() {
            SpacesPerIndent = 4;
            SpaceAfterComma = true;
            SpaceAfterSemiColonInFor = true;
            SpaceBeforeAndAfterBinaryOperator = true;
            SpaceAfterKeywordsInControlFlowStatements = true;
            SpaceAfterFunctionInAnonymousFunctions = true;
            NewLine = "\r\n";
        }
    }
}
