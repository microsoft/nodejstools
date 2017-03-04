// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;

namespace PythonToolsMockTests
{
    internal class MockIntellisenseSessionStack : IIntellisenseSessionStack
    {
        private readonly ObservableCollection<IIntellisenseSession> _stack = new ObservableCollection<IIntellisenseSession>();

        public void CollapseAllSessions()
        {
            _stack.Clear();
        }

        public void MoveSessionToTop(IIntellisenseSession session)
        {
            if (!_stack.Remove(session))
            {
                throw new InvalidOperationException();
            }
            PushSession(session);
        }

        public IIntellisenseSession PopSession()
        {
            if (_stack.Count == 0)
            {
                throw new InvalidOperationException();
            }
            var last = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            return last;
        }

        public void PushSession(IIntellisenseSession session)
        {
            session.Dismissed += session_Dismissed;
            _stack.Add(session);
        }

        private void session_Dismissed(object sender, EventArgs e)
        {
            var session = (IIntellisenseSession)sender;
            if (session == TopSession)
            {
                session.Dismissed -= session_Dismissed;
                PopSession();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public ReadOnlyObservableCollection<IIntellisenseSession> Sessions
        {
            get
            {
                return new ReadOnlyObservableCollection<IIntellisenseSession>(_stack);
            }
        }

        public IIntellisenseSession TopSession
        {
            get
            {
                if (_stack.Count == 0)
                {
                    return null;
                }
                return _stack[_stack.Count - 1];
            }
        }
    }
}

