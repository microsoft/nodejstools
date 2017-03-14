// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    [Export(typeof(IQuickInfoBroker))]
    internal class MockQuickInfoBroker : IQuickInfoBroker
    {
        public IQuickInfoSession CreateQuickInfoSession(VisualStudio.Text.Editor.ITextView textView, VisualStudio.Text.ITrackingPoint triggerPoint, bool trackMouse)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IQuickInfoSession> GetSessions(VisualStudio.Text.Editor.ITextView textView)
        {
            throw new NotImplementedException();
        }

        public bool IsQuickInfoActive(VisualStudio.Text.Editor.ITextView textView)
        {
            throw new NotImplementedException();
        }

        public IQuickInfoSession TriggerQuickInfo(VisualStudio.Text.Editor.ITextView textView, VisualStudio.Text.ITrackingPoint triggerPoint, bool trackMouse)
        {
            throw new NotImplementedException();
        }

        public IQuickInfoSession TriggerQuickInfo(VisualStudio.Text.Editor.ITextView textView)
        {
            throw new NotImplementedException();
        }
    }
}

