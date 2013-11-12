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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.NpmUI{
    public partial class BusyPopup : Form{
        public BusyPopup(){
            InitializeComponent();
            CreateHandle();
        }

        public string Message{
            get { return _busyControl.Message; }
            set { _busyControl.Message = value; }
        }

        private void Completed(){
            BeginInvoke(
                new Action(
                    () =>{
                        DialogResult = DialogResult.OK;
                        Close();
                    }));
        }

        public void ShowPopup(IWin32Window parent, Action action){
            Task t = new Task(action);
            t.ContinueWith(t2 => Completed());
            t.Start();

            ShowDialog(parent);
        }
    }
}
