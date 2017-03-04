// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockSignatureHelpSession : ISignatureHelpSession
    {
        private bool _dismissed;
        private readonly ITextView _view;
        private readonly ReadOnlyObservableCollection<ISignature> _sigs;
        private readonly ITrackingPoint _triggerPoint;
        private readonly PropertyCollection _properties = new PropertyCollection();
        private ISignature _active;

        public MockSignatureHelpSession(ITextView view, ObservableCollection<ISignature> sigs, ITrackingPoint triggerPoint)
        {
            _view = view;
            sigs.CollectionChanged += sigs_CollectionChanged;
            _triggerPoint = triggerPoint;
            _sigs = new ReadOnlyObservableCollection<ISignature>(sigs);
        }

        private void sigs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                throw new NotImplementedException();
            }
            if (_active == null)
            {
                _active = _sigs[0];
            }
        }

        public ISignature SelectedSignature
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        public event EventHandler<SelectedSignatureChangedEventArgs> SelectedSignatureChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public System.Collections.ObjectModel.ReadOnlyObservableCollection<ISignature> Signatures
        {
            get { return _sigs; }
        }

        public void Collapse()
        {
            throw new NotImplementedException();
        }

        public void Dismiss()
        {
            _dismissed = true;
            var dismissed = Dismissed;
            if (dismissed != null)
            {
                dismissed(this, EventArgs.Empty);
            }
        }

        public event EventHandler Dismissed;

        public VisualStudio.Text.SnapshotPoint? GetTriggerPoint(VisualStudio.Text.ITextSnapshot textSnapshot)
        {
            return GetTriggerPoint(textSnapshot.TextBuffer).GetPoint(textSnapshot);
        }

        public VisualStudio.Text.ITrackingPoint GetTriggerPoint(VisualStudio.Text.ITextBuffer textBuffer)
        {
            if (textBuffer == _triggerPoint.TextBuffer)
            {
                return _triggerPoint;
            }
            throw new NotImplementedException();
        }

        public bool IsDismissed
        {
            get { return _dismissed; }
        }

        public bool Match()
        {
            throw new NotImplementedException();
        }

        public IIntellisensePresenter Presenter
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler PresenterChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public void Recalculate()
        {
            throw new NotImplementedException();
        }

        public event EventHandler Recalculated
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public VisualStudio.Text.Editor.ITextView TextView
        {
            get { return _view; }
        }

        public VisualStudio.Utilities.PropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}

