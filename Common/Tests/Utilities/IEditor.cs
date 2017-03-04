// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace TestUtilities
{
    public interface IEditor
    {
        IIntellisenseSession TopSession
        {
            get;
        }
        string Text
        {
            get;
        }
        void Type(string text);

        void Invoke(Action action);

        void MoveCaret(int line, int column);
        void SetFocus();

        IWpfTextView TextView
        {
            get;
        }

        IClassifier Classifier
        {
            get;
        }

        void WaitForText(string text);
        void Select(int line, int column, int length);

        SessionHolder<T> WaitForSession<T>() where T : IIntellisenseSession;
        SessionHolder<T> WaitForSession<T>(bool assertIfNoSession) where T : IIntellisenseSession;

        void AssertNoIntellisenseSession();
    }
}

