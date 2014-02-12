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
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade {
    class IndentBasedOutlineRegionBuilder : OutlineRegionBuilder {
        struct CodeBlock {
            public int Start;
            public int Indent;

            public CodeBlock(int start, int indent) {
                Start = start;
                Indent = indent;
            }
        }

        private static int _minLinesToOutline = 2;

        public IndentBasedOutlineRegionBuilder(ITextBuffer textBuffer)
            : base(textBuffer) {
            BackgroundTask.DoTaskOnIdle();
        }

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            base.OnTextBufferChanged(sender, e);

            BackgroundTask.DoTaskOnIdle();
        }

        protected override void BuildRegions(OutlineRegionCollection newRegions) {
            BuildRegions(newRegions, TextBuffer.CurrentSnapshot);
        }

        private static void BuildRegions(OutlineRegionCollection newRegions, ITextSnapshot snapshot) {
            // Figure out regions based on line indent
            if (snapshot.LineCount == 0)
                return;

            var regionStack = new Stack<CodeBlock>();
            var lineLengths = new int[snapshot.LineCount];

            int lastBlockIndent = 0;

            for (int i = 0; i < snapshot.LineCount; i++) {
                var line = snapshot.GetLineFromLineNumber(i);

                var lineText = line.GetText();
                if (String.IsNullOrWhiteSpace(lineText)) {
                    lineLengths[i] = 0;
                    continue;
                }

                lineLengths[i] = line.Length;
                int indent = GetLineIndent(line);

                if (regionStack.Count > 0)
                    lastBlockIndent = regionStack.Peek().Indent;
                else
                    lastBlockIndent = 0;

                if (indent <= lastBlockIndent) {
                    // We add regions optimistically since any line can
                    // start a new region if lines below it are indented
                    // deeper that this line.
                    while (regionStack.Count > 0) {
                        // If we have line with the same indent, remove previously added region
                        // and replace it with a new one potentially starting with the current line.
                        var prevCodeBlock = regionStack.Pop();
                        int startLine = snapshot.GetLineNumberFromPosition(prevCodeBlock.Start);

                        // Trim empty lines
                        int j = i - 1;
                        for (; j >= 0; j--) {
                            if (lineLengths[j] > 0)
                                break;
                        }

                        j++;

                        if (j > 0 && j - startLine >= _minLinesToOutline) {
                            var prevLine = snapshot.GetLineFromLineNumber(j - 1);

                            if (prevCodeBlock.Start < prevLine.End)
                                newRegions.Add(OutlineRegion.FromBounds(snapshot.TextBuffer, prevCodeBlock.Start, prevLine.End));
                        }

                        if (regionStack.Count > 0) {
                            prevCodeBlock = regionStack.Peek();
                            if (prevCodeBlock.Indent < indent)
                                break;
                        }
                    }
                }

                lastBlockIndent = indent;
                regionStack.Push(new CodeBlock(line.Start, indent));
            }

            // Note that last region may be bogus since we add regions optimistically.
            // Remove last region if its indent is the same as the line before it
            if (regionStack.Count > 0) {
                var codeBlock = regionStack.Peek();
                var lineNumber = snapshot.GetLineNumberFromPosition(codeBlock.Start);
                if (lineNumber > 0) {
                    var prevLine = snapshot.GetLineFromLineNumber(lineNumber - 1);
                    int indent = GetLineIndent(prevLine);

                    if (indent == codeBlock.Indent)
                        regionStack.Pop();
                }
            }

            while (regionStack.Count > 0) {
                var codeBlock = regionStack.Pop();

                int startLine = snapshot.GetLineNumberFromPosition(codeBlock.Start);
                if (snapshot.LineCount - startLine >= _minLinesToOutline) {
                    newRegions.Add(OutlineRegion.FromBounds(snapshot.TextBuffer, codeBlock.Start, snapshot.Length));
                }
            }
        }

        private static int GetLineIndent(ITextSnapshotLine line) {
            for (int i = line.Start; i < line.End; i++) {
                char ch = line.Snapshot.GetText(i, 1)[0];
                if (!Char.IsWhiteSpace(ch))
                    return i - line.Start;
            }

            return 0;
        }
    }
}
