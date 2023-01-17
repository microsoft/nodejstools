// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using System.Globalization;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.NodejsTools.Extras;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IInteractiveWindowCommand))]
    [ContentType(InteractiveWindowContentType.ContentType)]
    internal class SaveReplCommand : InteractiveWindowCommand
    {
        public override Task<ExecutionResult> Execute(IInteractiveWindow window, string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                window.WriteError(Resources.ReplSaveNoFileName);
                return ExecutionResult.Failed;
            }
            else if (arguments.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.ReplSaveInvalidFileName, arguments));
                return ExecutionResult.Failed;
            }

            var text = new StringBuilder();

            var positions = new List<KeyValuePair<int, ITextBuffer>>();
            foreach (var buffer in window.TextView.BufferGraph.GetTextBuffers(IsJavaScriptBuffer))
            {
                var target = window.TextView.BufferGraph.MapUpToBuffer(
                    new SnapshotPoint(buffer.CurrentSnapshot, 0),
                    PointTrackingMode.Positive,
                    PositionAffinity.Successor,
                    window.TextView.TextBuffer
                );

                positions.Add(new KeyValuePair<int, ITextBuffer>(target.Value, buffer));
            }

            positions.Sort((x, y) => x.Key.CompareTo(y.Key));
            foreach (var buffer in positions)
            {
                var bufferText = buffer.Value.CurrentSnapshot.GetText();
                if (!bufferText.StartsWith(".", StringComparison.Ordinal))
                {
                    text.Append(bufferText);
                    text.Append(Environment.NewLine);
                }
            }

            try
            {
                File.WriteAllText(arguments, text.ToString());
                window.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.ReplSaveSucces, arguments));
            }
            catch
            {
                window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.ReplSaveFailed, arguments));
            }
            return ExecutionResult.Succeeded;
        }

        public override string Description => Resources.ReplSaveDescription;

        public override string Command => "save";

        private static bool IsJavaScriptBuffer(ITextBuffer buffer)
        {
            return buffer.ContentType.IsOfType(NodejsConstants.JavaScript);
        }
    }
}
