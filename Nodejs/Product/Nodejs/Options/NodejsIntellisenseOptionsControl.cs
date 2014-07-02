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
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsIntellisenseOptionsControl : UserControl {
        public NodejsIntellisenseOptionsControl() {
            InitializeComponent();
            AnalysisLevel = NodejsPackage.Instance.AdvancedOptionsPage.AnalysisLevel;
        }

        internal AnalysisLevel AnalysisLevel {
            get {
                if (_fullIntelliSenseRadioButton.Checked) {
                    return AnalysisLevel.High;
                } else if (_limitedIntelliSenseRadioButton.Checked) {
                    return AnalysisLevel.Low;
                } else {
                    return AnalysisLevel.None;
                }
            }
            set {
                switch (value) {
                    case AnalysisLevel.High:
                        _fullIntelliSenseRadioButton.Checked = true;
                        break;
                    case AnalysisLevel.Low:
                        _limitedIntelliSenseRadioButton.Checked = true;
                        break;
                    case AnalysisLevel.None:
                        _noIntelliSenseRadioButton.Checked = true;
                        break;
                    default:
                        Debug.Fail("Unrecognized AnalysisLevel: " + value);
                        break;
                }
            }
        }
    }
}
