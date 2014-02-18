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
    /// <summary>
    /// Encapsulates ther result of the text edits which come back from the
    /// TypeScript language service.
    /// </summary>
    class TextEdit {
        public readonly int MinChar, LimChar;
        public readonly string Text;

        public TextEdit(int minChar, int limChar, string text) {
            MinChar = minChar;
            LimChar = limChar;
            Text = text;
        }
    }
}
