// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Text;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// A helper class exposing various helper functions that 
    /// are used in formatting, smart indent and elsewhere else.
    /// </summary>
    internal static class TextHelper
    {
        public static string ConvertTabsToSpaces(string text, int tabSize, bool replaceNonWhitespaceChars = false)
        {
            var sb = new StringBuilder(text.Length);
            var charsSoFar = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == '\t')
                {
                    var spaces = tabSize - (charsSoFar % tabSize);
                    sb.Append(' ', spaces);
                    charsSoFar = 0;
                }
                else if (ch == '\r' || ch == '\n')
                {
                    charsSoFar = 0;
                    sb.Append(ch);
                }
                else
                {
                    charsSoFar++;
                    charsSoFar = charsSoFar % tabSize;
                    if (replaceNonWhitespaceChars)
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
