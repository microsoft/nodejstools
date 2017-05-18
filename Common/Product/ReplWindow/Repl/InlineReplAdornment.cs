// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IntraTextAdornmentTag))]
    [ContentType(ReplConstants.ReplContentTypeName)]
    internal class InlineReplAdornmentProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer == null || textView == null || typeof(T) != typeof(IntraTextAdornmentTag))
            {
                return null;
            }

            return (ITagger<T>)textView.Properties.GetOrCreateSingletonProperty<InlineReplAdornmentManager>(
                typeof(InlineReplAdornmentManager),
                () => new InlineReplAdornmentManager(textView)
                );
        }

        internal static InlineReplAdornmentManager GetManager(ITextView view)
        {
            InlineReplAdornmentManager result;
            if (!view.Properties.TryGetProperty<InlineReplAdornmentManager>(typeof(InlineReplAdornmentManager), out result))
            {
                return null;
            }
            return result;
        }

        public static void AddInlineAdornment(ITextView view, UIElement uiElement, RoutedEventHandler onLoaded, SnapshotPoint targetLoc)
        {
            var manager = GetManager(view);
            if (manager != null)
            {
                var adornment = new ZoomableInlineAdornment(uiElement, view);
                // Event is unhooked in ReplWindow.  If we don't we'll receive the event multiple
                // times leading to very jerky / hang like behavior where we've setup a new event
                // loop in the repl window.
                adornment.Loaded += onLoaded;
                manager.AddAdornment(adornment, targetLoc);
            }
        }

        public static void ZoomInlineAdornments(ITextView view, double zoomFactor)
        {
            var manager = GetManager(view);
            if (manager != null)
            {
                foreach (var t in manager.Adornments)
                {
                    t.Item2.Zoom(zoomFactor);
                }
            }
        }

        public static void MinimizeLastInlineAdornment(ITextView view)
        {
            var manager = GetManager(view);
            if (manager != null && manager.Adornments.Count > 0)
            {
                var adornment = manager.Adornments[manager.Adornments.Count - 1].Item2;
                adornment.Zoom(adornment.MinimizedZoom);
            }
        }

        public static void RemoveAllAdornments(ITextView view)
        {
            var manager = GetManager(view);
            if (manager != null)
            {
                manager.RemoveAll();
            }
        }
    }
}

