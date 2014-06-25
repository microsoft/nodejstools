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
using System.Text;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools {
    public static class Extensions {
        public static IEnumerable<IVsProject> EnumerateLoadedProjects(this IVsSolution solution, bool onlyNodeProjects = true) {
            var flags =
                onlyNodeProjects ?
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_MATCHTYPE) :
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_ALLVIRTUAL);
            var guid = new Guid(Guids.NodejsBaseProjectFactoryString);
            IEnumHierarchies hierarchies;
            ErrorHandler.ThrowOnFailure((solution.GetProjectEnum(
                flags,
                ref guid,
                out hierarchies)));
            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out fetched)) && fetched == 1) {
                var project = hierarchy[0] as IVsProject;
                if (project != null) {
                    yield return project;
                }
            }
        }

        internal static IEnumerable<uint> EnumerateProjectItems(this IVsProject project) {
            var enumHierarchyItemsFactory = Package.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) as IVsEnumHierarchyItemsFactory;
            var hierarchy = (IVsHierarchy)project;
            if (enumHierarchyItemsFactory != null && project != null) {
                IEnumHierarchyItems enumHierarchyItems;
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_OmitHier),
                        (uint)VSConstants.VSITEMID_ROOT,
                        out enumHierarchyItems))) {
                    if (enumHierarchyItems != null) {
                        VSITEMSELECTION[] rgelt = new VSITEMSELECTION[1];
                        uint fetched;
                        while (VSConstants.S_OK == enumHierarchyItems.Next(1, rgelt, out fetched) && fetched == 1) {
                            yield return rgelt[0].itemid;
                        }
                    }
                }
            }
        }

        internal static NodejsProjectNode GetNodeProject(this EnvDTE.Project project) {
            return project.GetCommonProject() as NodejsProjectNode;
        }

        internal static EnvDTE.Project GetProject(this IVsHierarchy hierarchy) {
            object project;

            ErrorHandler.ThrowOnFailure(
                hierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out project
                )
            );

            return (project as EnvDTE.Project);
        }

        internal static string GetFilePath(this ITextBuffer textBuffer) {
            ITextDocument textDocument;
            if (textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument)) {
                return textDocument.FilePath;
            } else {
                return null;
            }
        }

        internal static T[] Append<T>(this T[] list, T item) {
            T[] res = new T[list.Length + 1];
            list.CopyTo(res, 0);
            res[res.Length - 1] = item;
            return res;
        }

        internal static EnvDTE.Project GetProject(this ITextBuffer buffer) {
            var path = buffer.GetFilePath();
            if (path != null && NodejsPackage.Instance != null) {
                foreach (EnvDTE.Project proj in NodejsPackage.Instance.DTE.Solution.Projects) {
                    var nodeProj = proj.GetNodeProject();
                    if (nodeProj != null) {
                        var nodeFile = nodeProj.FindNodeByFullPath(path);
                        if (nodeFile != null) {
                            return proj;
                        }
                    }
                }
                var item = NodejsPackage.Instance.DTE.Solution.FindProjectItem(path);
                if (item != null) {
                    return item.ContainingProject;
                }
            }
            return null;
        }

        internal static NodejsProjectNode GetNodejsProject(this EnvDTE.Project project) {
            return project.GetCommonProject() as NodejsProjectNode;
        }

        internal static VsProjectAnalyzer GetAnalyzer(this ITextView textView) {
            NodejsReplEvaluator evaluator;
            if (textView.Properties.TryGetProperty<NodejsReplEvaluator>(typeof(NodejsReplEvaluator), out evaluator)) {
                //return evaluator.ReplAnalyzer;
                throw new NotImplementedException("TODO: Repl analysis");
            }
            return textView.TextBuffer.GetAnalyzer();
        }

        internal static VsProjectAnalyzer GetAnalyzer(this ITextBuffer buffer) {
            NodejsProjectNode pyProj;
            VsProjectAnalyzer analyzer;
            if (!buffer.Properties.TryGetProperty<NodejsProjectNode>(typeof(NodejsProjectNode), out pyProj)) {
                var project = buffer.GetProject();
                if (project != null) {
                    pyProj = project.GetNodejsProject();
                    if (pyProj != null) {
                        buffer.Properties.AddProperty(typeof(NodejsProjectNode), pyProj);
                    }
                }
            }

            if (pyProj != null) {
                analyzer = pyProj.Analyzer;
                return analyzer;
            }

            // exists for tests where we don't run in VS and for the existing changes preview
            if (buffer.Properties.TryGetProperty<VsProjectAnalyzer>(typeof(VsProjectAnalyzer), out analyzer)) {
                return analyzer;
            }

            return NodejsPackage.Instance.DefaultAnalyzer;
        }

        internal static bool TryGetJsProjectEntry(this ITextBuffer buffer, out IJsProjectEntry entry) {
            IProjectEntry e;
            if (buffer.TryGetProjectEntry(out e) && (entry = e as IJsProjectEntry) != null) {
                return true;
            }
            entry = null;
            return false;
        }

        internal static IProjectEntry GetProjectEntry(this ITextBuffer buffer) {
            IProjectEntry res;
            buffer.TryGetProjectEntry(out res);
            return res;
        }

        internal static bool TryGetProjectEntry(this ITextBuffer buffer, out IProjectEntry entry) {
            return buffer.Properties.TryGetProperty<IProjectEntry>(typeof(IProjectEntry), out entry);
        }

        internal static string LimitLines(
            this string str,
            int maxLines = 30,
            int charsPerLine = 200,
            bool ellipsisAtEnd = true,
            bool stopAtFirstBlankLine = false
        ) {
            if (string.IsNullOrEmpty(str)) {
                return str;
            }

            int lineCount = 0;
            var prettyPrinted = new StringBuilder();
            bool wasEmpty = true;

            using (var reader = new StringReader(str)) {
                for (var line = reader.ReadLine(); line != null && lineCount < maxLines; line = reader.ReadLine()) {
                    if (string.IsNullOrWhiteSpace(line)) {
                        if (wasEmpty) {
                            continue;
                        }
                        wasEmpty = true;
                        if (stopAtFirstBlankLine) {
                            lineCount = maxLines;
                            break;
                        }
                        lineCount += 1;
                        prettyPrinted.AppendLine();
                    } else {
                        wasEmpty = false;
                        lineCount += (line.Length / charsPerLine) + 1;
                        prettyPrinted.AppendLine(line);
                    }
                }
            }
            if (ellipsisAtEnd && lineCount >= maxLines) {
                prettyPrinted.AppendLine("...");
            }
            return prettyPrinted.ToString().Trim();
        }

        internal static bool CanComplete(this ClassificationSpan token) {
            return token.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Keyword) |
                token.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Identifier);
        }

        internal static bool IsOpenGrouping(this ClassificationSpan span) {
            return span.ClassificationType.IsOfType(NodejsPredefinedClassificationTypeNames.Grouping) &&
                span.Span.Length == 1 &&
                (span.Span.GetText() == "{" || span.Span.GetText() == "[" || span.Span.GetText() == "(");
        }

        internal static bool IsCloseGrouping(this ClassificationSpan span) {
            return span.ClassificationType.IsOfType(NodejsPredefinedClassificationTypeNames.Grouping) &&
                span.Span.Length == 1 &&
                (span.Span.GetText() == "}" || span.Span.GetText() == "]" || span.Span.GetText() == ")");
        }

        internal static void GotoSource(this LocationInfo location) {
            NodejsPackage.NavigateTo(
                location.FilePath,
                Guid.Empty,
                location.Line - 1,
                location.Column - 1
            );            
        }

        internal static SnapshotPoint? GetCaretPosition(this ITextView view) {
            return view.BufferGraph.MapDownToFirstMatch(
               new SnapshotPoint(view.TextBuffer.CurrentSnapshot, view.Caret.Position.BufferPosition),
               PointTrackingMode.Positive,
               IntellisenseController.IsNodejsContent,
               PositionAffinity.Successor
            );
        }

        private static bool IsIdentifierChar(char curChar) {
            return Char.IsLetterOrDigit(curChar) || curChar == '_' || curChar == '$';
        }

        internal static ITrackingSpan GetCaretSpan(this ITextView view) {
            var caretPoint = view.GetCaretPosition();
            Debug.Assert(caretPoint != null);
            var snapshot = caretPoint.Value.Snapshot;
            var caretPos = caretPoint.Value.Position;

            // fob(
            //    ^
            //    +---  Caret here
            //
            // We want to lookup fob, not fob(
            //
            ITrackingSpan span;
            if (caretPos != snapshot.Length) {
                string curChar = snapshot.GetText(caretPos, 1);
                if (!IsIdentifierChar(curChar[0]) && caretPos > 0) {
                    string prevChar = snapshot.GetText(caretPos - 1, 1);
                    if (IsIdentifierChar(prevChar[0])) {
                        caretPos--;
                    }
                }
                span = snapshot.CreateTrackingSpan(
                    caretPos,
                    1,
                    SpanTrackingMode.EdgeInclusive
                );
            } else {
                span = snapshot.CreateTrackingSpan(
                    caretPos,
                    0,
                    SpanTrackingMode.EdgeInclusive
                );
            }

            return span;
        }

        internal static ITrackingSpan CreateTrackingSpan(this IQuickInfoSession session, ITextBuffer buffer) {
            var triggerPoint = session.GetTriggerPoint(buffer);
            var position = triggerPoint.GetPosition(buffer.CurrentSnapshot);
            if (position == buffer.CurrentSnapshot.Length) {
                return ((IIntellisenseSession)session).GetApplicableSpan(buffer);
            }

            return buffer.CurrentSnapshot.CreateTrackingSpan(position, 1, SpanTrackingMode.EdgeInclusive);
        }

        /// <summary>
        /// Returns the span to use for the provided intellisense session.
        /// </summary>
        /// <returns>A tracking span. The span may be of length zero if there
        /// is no suitable token at the trigger point.</returns>
        internal static ITrackingSpan GetApplicableSpan(this IIntellisenseSession session, ITextBuffer buffer) {
            var snapshot = buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(buffer);

            var span = snapshot.GetApplicableSpan(triggerPoint);
            if (span != null) {
                return span;
            }
            return snapshot.CreateTrackingSpan(triggerPoint.GetPosition(snapshot), 0, SpanTrackingMode.EdgeInclusive);
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        internal static ITrackingSpan GetApplicableSpan(this ITextSnapshot snapshot, ITrackingPoint point) {
            return snapshot.GetApplicableSpan(point.GetPosition(snapshot));
        }

        /// <summary>
        /// Returns the applicable span at the provided position.
        /// </summary>
        /// <returns>A tracking span, or null if there is no token at the
        /// provided position.</returns>
        internal static ITrackingSpan GetApplicableSpan(this ITextSnapshot snapshot, int position) {
            var classifier = snapshot.TextBuffer.GetNodejsClassifier();
            var line = snapshot.GetLineFromPosition(position);
            if (classifier == null || line == null) {
                return null;
            }

            var spanLength = position - line.Start.Position;
            // Increase position by one to include 'fob' in: "abc.|fob"
            if (spanLength < line.Length) {
                spanLength += 1;
            }

            var classifications = classifier.GetClassificationSpans(new SnapshotSpan(line.Start, spanLength));
            // Handle "|"
            if (classifications == null || classifications.Count == 0) {
                return null;
            }

            var lastToken = classifications[classifications.Count - 1];
            // Handle "fob |"
            if (lastToken == null || position > lastToken.Span.End) {
                return null;
            }

            if (position > lastToken.Span.Start) {
                if (lastToken.CanComplete()) {
                    // Handle "fo|o"
                    return snapshot.CreateTrackingSpan(lastToken.Span, SpanTrackingMode.EdgeInclusive);
                } else {
                    // Handle "<|="
                    return null;
                }
            }

            var secondLastToken = classifications.Count >= 2 ? classifications[classifications.Count - 2] : null;
            if (lastToken.Span.Start == position && lastToken.CanComplete() &&
                (secondLastToken == null ||             // Handle "|fob"
                 position > secondLastToken.Span.End || // Handle "if |fob"
                 !secondLastToken.CanComplete())) {     // Handle "abc.|fob"
                return snapshot.CreateTrackingSpan(lastToken.Span, SpanTrackingMode.EdgeInclusive);
            }

            // Handle "abc|."
            // ("ab|c." would have been treated as "ab|c")
            if (secondLastToken != null && secondLastToken.Span.End == position && secondLastToken.CanComplete()) {
                return snapshot.CreateTrackingSpan(secondLastToken.Span, SpanTrackingMode.EdgeInclusive);
            }

            return null;
        }
    }
}
