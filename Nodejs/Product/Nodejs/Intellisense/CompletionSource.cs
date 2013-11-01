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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Intellisense {
    sealed partial class CompletionSource : ICompletionSource {
        private readonly ITextBuffer _textBuffer;
        private readonly IClassifier _classifier;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGlyphService _glyphService;

        private static string[] _allowRequireTokens = new[] { "!", "!=", "!==", "%", "%=", "&", "&&", "&=", "(", ")", 
            "*", "*=", "+", "++", "+=", ",", "-", "--", "-=",  "..", "...", "/", "/=", ":", ";", "<", "<<", "<<=", 
            "<=", "=", "==", "===", ">", ">=", ">>", ">>=", ">>>", ">>>=", "?", "[", "^", "^=", "{", "|", "|=", "||", 
            "}", "~", "in", "case", "new", "return", "throw", "typeof"
        };

        private static string[] _keywords = new[] {
            "break", "case", "catch", "class", "const", "continue", "default", "delete", "do", "else", "eval", "extends", 
            "false", "field", "final", "finally", "for", "function", "if", "import", "in", "instanceof", "new", "null", 
            "package", "private", "protected", "public", "return", "super", "switch", "this", "throw", "true", "try", 
            "typeof", "var", "while", "with",
            "abstract", "debugger", "enum", "export", "goto", "implements", "native", "static", "synchronized", "throws",
            "transient", "volatile"
        };

        public CompletionSource(ITextBuffer textBuffer, IClassifierAggregatorService classifierAggregator, IServiceProvider serviceProvider, IGlyphService glyphService) {
            _textBuffer = textBuffer;
            _classifier = classifierAggregator.GetClassifier(textBuffer);
            _serviceProvider = serviceProvider;
            _glyphService = glyphService;
        }

        #region ICompletionSource Members

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets) {
            var buffer = _textBuffer;
            var triggerPoint = session.GetTriggerPoint(buffer).GetPoint(buffer.CurrentSnapshot);
            bool shouldTrigger = ShouldTriggerRequireIntellisense(triggerPoint, _classifier, true, true);

            if (shouldTrigger) {
                var classifications = EnumerateClassificationsInReverse(_classifier, triggerPoint);
                bool? doubleQuote = null;
                int length = 0;

                // check which one of these we're doing:
                // require(         inserting 'module' at trigger point
                // require('        inserting module' at trigger point
                // requre('ht')     ctrl space at ht, inserting http' at trigger point - 2
                // requre('addo')   ctrl space at add, inserting addons' at trigger point - 3

                // Therefore we have no quotes or quotes.  In no quotes we insert both
                // leading and trailing quotes.  In quotes we leave the leading quote in
                // place and replace any other quotes value that was already there.

                if (classifications.MoveNext()) {
                    var curText = classifications.Current.Span.GetText();
                    if (curText.StartsWith("'") || curText.StartsWith("\"")) {
                        // we're in the quotes case, figure out the existing string,
                        // and use that at the applicable span.
                        var fullSpan = _classifier.GetClassificationSpans(
                            new SnapshotSpan(
                                classifications.Current.Span.Start,
                                classifications.Current.Span.End.GetContainingLine().End
                            )
                        ).First();

                        doubleQuote = curText[0] == '"';
                        triggerPoint -= (curText.Length - 1);
                        length = fullSpan.Span.Length - 1;
                    }
                    // else it's require(
                }

                var completions = GenerateBuiltinCompletions(doubleQuote);
                GetProjectCompletions(completions, doubleQuote);
                completions.Sort(CompletionSorter);

                completionSets.Add(
                    new CompletionSet(
                        "Node.js require",
                        "Node.js require",
                        _textBuffer.CurrentSnapshot.CreateTrackingSpan(
                            triggerPoint,
                            length,
                            SpanTrackingMode.EdgeInclusive
                        ),
                        completions,
                        null
                    )
                );
            }
        }

        private int CompletionSorter(Completion x, Completion y) {
            if (x.DisplayText.StartsWith(".")) {
                if (y.DisplayText.StartsWith(".")) {
                    String.Compare(x.DisplayText, y.DisplayText);
                }
                return 1;
            } else if (y.DisplayText.StartsWith(".")) {
                return -1;
            }
            return String.Compare(x.DisplayText, y.DisplayText);
        }

        /// <summary>
        /// Checks if we are at a require statement where we can offer completions.
        /// 
        /// The bool flags are used to control when we are checking if we should provide
        /// the completions before updating the buffer the characters the user just typed.
        /// </summary>
        /// <param name="triggerPoint">The point where the completion session is being triggered</param>
        /// <param name="classifier">A classifier for getting the tokens</param>
        /// <param name="eatOpenParen">True if the open paren has been inserted and we should expect it</param>
        /// <param name="allowQuote">True if we will parse the require(' or require(" forms.</param>
        /// <returns></returns>
        internal static bool ShouldTriggerRequireIntellisense(SnapshotPoint triggerPoint, IClassifier classifier, bool eatOpenParen, bool allowQuote = false) {
            var classifications = EnumerateClassificationsInReverse(classifier, triggerPoint);
            bool atRequire = false;

            if (allowQuote && classifications.MoveNext()) {
                var curText = classifications.Current.Span.GetText();
                if (!curText.StartsWith("'") && !curText.StartsWith("\"")) {
                    // no leading quote, reset back to original classifications.
                    classifications = EnumerateClassificationsInReverse(classifier, triggerPoint);
                }
            }

            if ((!eatOpenParen || EatToken(classifications, "(")) && EatToken(classifications, "require")) {
                // start of a file or previous token to require is followed by an expression
                if (!classifications.MoveNext()) {
                    // require at beginning of the file
                    atRequire = true;
                } else {
                    var tokenText = classifications.Current.Span.GetText();

                    atRequire =
                        tokenText.EndsWith(";") || // f(x); has ); displayed as a single token
                        _allowRequireTokens.Contains(tokenText) || // require after a token which starts an expression
                        (tokenText.All(IsIdentifierChar) && !_keywords.Contains(tokenText));    // require after anything else that isn't a statement like keyword 
                                                                                                //      (including identifiers which are on the previous line)
                }
            }
            
            return atRequire;
        }

        internal static bool IsIdentifierChar(char ch) {
            return ch == '_' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9');
        }


        private void GetProjectCompletions(List<Completion> completions, bool? doubleQuote) {
            var projBuffer = _textBuffer.Properties.GetProperty<NodejsProjectionBuffer>(typeof(NodejsProjectionBuffer));
            var filePath = projBuffer.DiskBuffer.GetFilePath();

            var rdt = _serviceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            IVsHierarchy hierarchy;
            uint itemId;
            IntPtr punk = IntPtr.Zero;
            uint cookie;
            int hr;
            try {
                hr = rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, filePath, out hierarchy, out itemId, out punk, out cookie);
                if (ErrorHandler.Succeeded(hr) && hierarchy != null) {
                    var dteProj = hierarchy.GetProject();
                    if (dteProj != null) {
                        var nodeProj = dteProj.GetNodeProject();
                        if (nodeProj != null) {
                            // file is open in our project, we can provide completions...
                            var node = nodeProj.FindNodeByFullPath(filePath);
                            Debug.Assert(node != null);

                            GetParentNodeModules(nodeProj, node.Parent, doubleQuote, completions);
                            GetPeerAndChildModules(nodeProj, node, doubleQuote, completions);
                        }
                    }
                }
            } finally {
                if (punk != IntPtr.Zero) {
                    Marshal.Release(punk);
                }
            }
        }

        private void GetParentNodeModules(NodejsProjectNode nodeProj, HierarchyNode parent, bool? doubleQuote, List<Completion> projectCompletions) {
            do {
                var modulesFolder = parent.FindImmediateChildByName(NodejsConstants.NodeModulesFolder);
                if (modulesFolder != null) {
                    GetParentNodeModules(nodeProj, modulesFolder, parent, doubleQuote, projectCompletions);
                }
                parent = parent.Parent;
            } while (parent != null);
        }

        private void GetParentNodeModules(NodejsProjectNode nodeProj, HierarchyNode modulesFolder, HierarchyNode fromFolder, bool? doubleQuote, List<Completion> projectCompletions) {
            for (HierarchyNode n = modulesFolder.FirstChild; n != null; n = n.NextSibling) {
                FileNode file = n as FileNode;
                if (file != null &&
                    NodejsConstants.FileExtension.Equals(
                        Path.GetExtension(file.Url),
                        StringComparison.OrdinalIgnoreCase
                    )) {
                    projectCompletions.Add(
                        MakeCompletion(
                            nodeProj,
                            doubleQuote,
                            file,
                            MakeNodePath(fromFolder, file).Substring(NodejsConstants.NodeModulesFolder.Length + 1)
                        )
                    );
                }

                FolderNode folder = n as FolderNode;
                if (folder != null) {
                    var package = folder.FindImmediateChildByName("package.json");
                    if (package != null) {
                        projectCompletions.Add(
                            MakeCompletion(
                                nodeProj,
                                doubleQuote,
                                folder,
                                MakeNodePath(fromFolder, folder).Substring(NodejsConstants.NodeModulesFolder.Length + 1)
                            )
                        );
                        // we don't recurse here - you can pull out a random .js file from a package, 
                        // but we don't include those in the available completions.
                    } else if (!NodejsConstants.NodeModulesFolder.Equals(Path.GetFileName(folder.Url), StringComparison.OrdinalIgnoreCase)) {
                        // recurse into folder and make available members...
                        GetParentNodeModules(nodeProj, folder, fromFolder, doubleQuote, projectCompletions);
                    }
                }
            }
        }

        private static string MakeNodePath(HierarchyNode relativeTo, HierarchyNode node) {
            return CommonUtils.CreateFriendlyFilePath(relativeTo.FullPathToChildren, CommonUtils.TrimEndSeparator(node.Url)).Replace("\\", "/");
        }

        private Completion MakeCompletion(NodejsProjectNode nodeProj, bool? doubleQuote, HierarchyNode node, string displayText) {
            return new Completion(
                displayText,
                GetInsertionQuote(doubleQuote, displayText),
                CommonUtils.CreateFriendlyFilePath(nodeProj.ProjectHome, node.Url) + " (in project)",
                _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupModule, StandardGlyphItem.GlyphItemProtected),
                null
            );
        }

        /// <summary>
        /// Finds available modules which are children of the folder where we're doing require
        /// completions from.
        /// </summary>
        private void GetPeerAndChildModules(NodejsProjectNode nodeProj, HierarchyNode node, bool? doubleQuote, List<Completion> projectCompletions) {
            var folder = node.Parent;
            foreach (var child in EnumCodeFilesExcludingNodeModules(folder)) {
                if (child == node) {
                    // you can require yourself, but we don't show the completion
                    continue;
                }

                projectCompletions.Add(
                    MakeCompletion(
                        nodeProj,
                        doubleQuote,
                        child,
                        "./" + MakeNodePath(folder, child)
                    )
                );
            }
        }

        /// <summary>
        /// Enumerates the available code files excluding node modules whih we handle specially.
        /// </summary>
        internal IEnumerable<NodejsFileNode> EnumCodeFilesExcludingNodeModules(HierarchyNode node) {
            for (HierarchyNode n = node.FirstChild; n != null; n = n.NextSibling) {
                var fileNode = n as NodejsFileNode;
                if (fileNode != null) {
                    yield return fileNode;
                }

                var folder = n as FolderNode;
                // exclude node_modules, you can do require('./node_modules/foo.js'), but no
                // one does.
                if (folder != null) {
                    var folderName = Path.GetFileName(CommonUtils.TrimEndSeparator(folder.Url));
                    if (!folderName.Equals(NodejsConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase)) {
                        foreach (var childNode in EnumCodeFilesExcludingNodeModules(n)) {
                            yield return childNode;
                        }
                    }
                }
            }
        }

        private static string GetInsertionQuote(bool? doubleQuote, string filename) {
            return doubleQuote == null ?
                "\'" + filename + "\'" :
                doubleQuote.Value ? filename + "\"" : filename + "'";
        }

        private static bool EatToken(IEnumerator<ClassificationSpan> classifications, string tokenText) {
            return classifications.MoveNext() && classifications.Current.Span.GetText() == tokenText;
        }

        /// <summary>
        /// Enumerates all of the classifications in reverse starting at start to the beginning of the file.
        /// </summary>
        private static IEnumerator<ClassificationSpan> EnumerateClassificationsInReverse(IClassifier classifier, SnapshotPoint start) {
            var curLine = start.GetContainingLine();
            var spanEnd = start;

            for (; ; ) {
                var classifications = classifier.GetClassificationSpans(new SnapshotSpan(curLine.Start, spanEnd));
                for (int i = classifications.Count - 1; i >= 0; i--) {
                    yield return classifications[i];
                }

                if (curLine.LineNumber == 0) {
                    break;
                }

                curLine = start.Snapshot.GetLineFromLineNumber(curLine.LineNumber - 1);
                spanEnd = curLine.End;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion

        private List<Completion> GenerateBuiltinCompletions(bool? doubleQuote) {
            var modules = _nodejsModules.Keys.ToArray();

            List<Completion> res = new List<Completion>();
            foreach (var module in modules) {
                res.Add(
                    new Completion(
                        module,
                        GetInsertionQuote(doubleQuote, module),
                        _nodejsModules[module],
                        _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupModule, StandardGlyphItem.GlyphItemPublic),
                        null
                    )
                );
            }
            return res;
        }
    }
}
