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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI {
    class NpmOutputControlViewModel : INotifyPropertyChanged {
        private INpmController _npmController;
        private string _statusText = Resources.NpmStatusReady;
        private bool _isExecutingCommand;

        public NpmOutputControlViewModel() {}

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public INpmController NpmController {
            get { return _npmController; }
            set {
                _npmController = value;
                OnPropertyChanged();
            }
        }

        public string StatusText {
            get { return _statusText; }
            set {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public bool IsExecutingCommand {
            get { return _isExecutingCommand; }
            set {
                _isExecutingCommand = value;
                OnPropertyChanged();
                OnPropertyChanged("ExecutionProgressVisibility");
            }
        }

        public Visibility ExecutionProgressVisibility {
            get { return IsExecutingCommand ? Visibility.Visible : Visibility.Hidden; }
        }

    }
}
