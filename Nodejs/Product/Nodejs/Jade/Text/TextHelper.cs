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
using System.Text;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// A helper class exposing various helper functions that 
    /// are used in formatting, smart indent and elsewhere else.
    /// </summary>
    static class TextHelper {
        /// <summary>
        /// Detemines if there is nothing but whitespace between
        /// given position and preceding line break or beginning 
        /// of the file.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Position to check</param>
        public static bool IsNewLineBeforePosition(ITextProvider textProvider, int position) {
            if (position == 0)
                return false;

            // Walk backwards from the artifact position
            for (int i = position - 1; i >= 0; i--) {
                char ch = textProvider[i];

                if (ch == '\n' || ch == '\r')
                    return true;

                if (!Char.IsWhiteSpace(ch))
                    break;
            }

            return false;
        }

        /// <summary>
        /// Determines if there is nothing but whitespace between
        /// given position and the next line break or end of file.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="position">Position to check</param>
        public static bool IsNewLineAfterPosition(ITextProvider textProvider, int position) {
            // Walk backwards from the artifact position
            for (int i = position; i < textProvider.Length; i++) {
                char ch = textProvider[i];

                if (ch == '\n' || ch == '\r')
                    return true;

                if (!Char.IsWhiteSpace(ch))
                    break;
            }

            return false;
        }

        /// <summary>
        /// Splits string into lines based on line breaks
        /// </summary>
        public static IList<string> SplitTextIntoLines(string text) {
            var lines = new List<string>();
            int lineStart = 0;

            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];
                if (ch == '\r' || ch == '\n') {
                    lines.Add(text.Substring(lineStart, i - lineStart));

                    if (i < text.Length - 1) {
                        ch = text[i + 1];

                        if (ch == '\r' || ch == '\n')
                            i++;
                    }

                    lineStart = i + 1;
                }
            }

            lines.Add(text.Substring(lineStart, text.Length - lineStart));

            return lines;
        }

        public static string ConvertTabsToSpaces(string text, int tabSize) {
            var sb = new StringBuilder(text.Length);
            int charsSoFar = 0;

            for (int i = 0; i < text.Length; i++) {
                char ch = text[i];

                if (ch == '\t') {
                    var spaces = tabSize - (charsSoFar % tabSize);
                    sb.Append(' ', spaces);
                    charsSoFar = 0;
                } else if (ch == '\r' || ch == '\n') {
                    charsSoFar = 0;
                    sb.Append(ch);
                } else {
                    charsSoFar++;
                    charsSoFar = charsSoFar % tabSize;
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        public static string RemoveIndent(string text, int tabSize) {
            // Normalize to spaces
            text = ConvertTabsToSpaces(text, tabSize);

            var lines = TextHelper.SplitTextIntoLines(text);

            // Measure how much whitespace is before each line and find minimal whitespace 
            // properly counting tabs and spaces. We convert tab to spaces when counting.
            // Store leading whitespace length for each line.

            var leadingWSLengthInChars = new int[lines.Count];

            for (int i = 0; i < lines.Count; i++) {
                var line = lines[i];

                leadingWSLengthInChars[i] = 0;

                if (line.Length > 0 && !String.IsNullOrWhiteSpace(line)) {
                    for (int j = 0; j < line.Length; j++) {
                        char ch = line[j];

                        if (!Char.IsWhiteSpace(ch))
                            break;

                        leadingWSLengthInChars[i]++;
                    }
                } else {
                    leadingWSLengthInChars[i] = Int32.MaxValue;
                }
            }

            int minWsInChars = Int32.MaxValue;
            for (int i = 0; i < lines.Count; i++) {
                minWsInChars = Math.Min(minWsInChars, leadingWSLengthInChars[i]);
            }

            // Now we know line wth smallest leading whitespace. We need to trim other lines 
            // leading whitespace by this amount and convert remaining leading whitespace
            // to tabs or spaces according to the formatting options.
            // Generate indenting whitespace for each line according to base block indent 
            // and current formatting options.

            var sb = new StringBuilder();

            for (int i = 0; i < lines.Count; i++) {
                var line = lines[i];

                if (!String.IsNullOrEmpty(line) && leadingWSLengthInChars[i] != Int32.MaxValue) {
                    sb.Append(lines[i].Substring(minWsInChars));
                }

                if (i < lines.Count - 1) {
                    sb.Append("\r\n");
                }
            }

            return sb.ToString();
        }
    }
}
