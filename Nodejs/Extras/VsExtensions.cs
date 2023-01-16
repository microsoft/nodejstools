// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools.Project;
using VsShellUtil = Microsoft.VisualStudio.Shell.VsShellUtilities;

namespace Microsoft.VisualStudioTools
{
    internal static class VsExtensions
    {
        public static string GetFilePath(this ITextView textView)
        {
            return textView.TextBuffer.GetFilePath();
        }

        internal static EnvDTE.Project GetProject(this IVsHierarchy hierarchy)
        {

            ErrorHandler.ThrowOnFailure(
                hierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out var project
                )
            );

            return (project as EnvDTE.Project);
        }

        public static string GetRootCanonicalName(this IVsProject project)
        {
            return ((IVsHierarchy)project).GetRootCanonicalName();
        }

        public static string GetRootCanonicalName(this IVsHierarchy heirarchy)
        {
            ErrorHandler.ThrowOnFailure(heirarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out var path));
            return path;
        }

        internal static T[] Append<T>(this T[] list, T item)
        {
            var res = new T[list.Length + 1];
            list.CopyTo(res, 0);
            res[res.Length - 1] = item;
            return res;
        }

        internal static string GetFilePath(this ITextBuffer textBuffer)
        {
            if (textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out var textDocument))
            {
                return textDocument.FilePath;
            }
            else
            {
                return null;
            }
        }

        internal static ClipboardServiceBase GetClipboardService(this IServiceProvider serviceProvider)
        {
            return (ClipboardServiceBase)serviceProvider.GetService(typeof(ClipboardServiceBase));
        }

        internal static UIThreadBase GetUIThread(this IServiceProvider serviceProvider)
        {
            var uiThread = (UIThreadBase)serviceProvider.GetService(typeof(UIThreadBase));
            if (uiThread == null)
            {
                Trace.TraceWarning("Returning NoOpUIThread instance from GetUIThread");
                Debug.Assert(VsShellUtil.ShellIsShuttingDown, "No UIThread service but shell is not shutting down");
                return new NoOpUIThread();
            }
            return uiThread;
        }

        [Conditional("DEBUG")]
        public static void MustBeCalledFromUIThread(this UIThreadBase self, string message = "Invalid cross-thread call")
        {
            Debug.Assert(self is MockUIThreadBase || !self.InvokeRequired, message);
        }

        [Conditional("DEBUG")]
        public static void MustNotBeCalledFromUIThread(this UIThreadBase self, string message = "Invalid cross-thread call")
        {
            Debug.Assert(self is MockUIThreadBase || self.InvokeRequired, message);
        }

        #region NoOpUIThread class

        /// <summary>
        /// Provides a no-op implementation of <see cref="UIThreadBase"/> that will
        /// not execute any tasks.
        /// </summary>
        private sealed class NoOpUIThread : MockUIThreadBase
        {
            public override void Invoke(Action action) { }

            public override T Invoke<T>(Func<T> func)
            {
                return default(T);
            }

            public override Task InvokeAsync(Action action)
            {
                return Task.FromResult<object>(null);
            }

            public override Task<T> InvokeAsync<T>(Func<T> func)
            {
                return Task.FromResult<T>(default(T));
            }

            public override Task InvokeTask(Func<Task> func)
            {
                return Task.FromResult<object>(null);
            }

            public override Task<T> InvokeTask<T>(Func<Task<T>> func)
            {
                return Task.FromResult<T>(default(T));
            }

            public override void MustBeCalledFromUIThreadOrThrow() { }

            public override bool InvokeRequired => false;
        }

        #endregion

        /// <summary>
        /// Use the line ending of the first line for the line endings.  
        /// If we have no line endings (single line file) just use Environment.NewLine
        /// </summary>
        public static string GetNewLineText(ITextSnapshot snapshot)
        {
            // https://nodejstools.codeplex.com/workitem/1670 : override the GetNewLineCharacter as VS always returns '\r\n'
            // check on each format as the user could have changed line endings (manually or through advanced save options) since
            // the file was opened.
            if (snapshot.LineCount > 0 && snapshot.GetLineFromPosition(0).LineBreakLength > 0)
            {
                return snapshot.GetLineFromPosition(0).GetLineBreakText();
            }
            else
            {
                return Environment.NewLine;
            }
        }
    }
}
