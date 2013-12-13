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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Stores cached information for a completion that we can easily transform
    /// into our completions.  Primarily this exists to account for whether or not
    /// the completion is happening with ', ", or no quotes while allowing us to
    /// cache the results we've previously calculated.
    /// </summary>
    class CompletionInfo {
        public readonly string DisplayText, Description;
        public readonly ImageSource Glyph;

        public CompletionInfo(string displayText, string description, ImageSource glyph) {
            DisplayText = displayText;
            Description = description;
            Glyph = glyph;
        }

        public Completion ToCompletion(bool? doubleQuote) {
            return new Completion(
                DisplayText,
                GetInsertionQuote(doubleQuote, DisplayText),
                Description,
                Glyph,
                null
            );
        }

        internal static string GetInsertionQuote(bool? doubleQuote, string filename) {
            return doubleQuote == null ?
                "\'" + filename + "\'" :
                doubleQuote.Value ? filename + "\"" : filename + "'";
        }
    }
}
