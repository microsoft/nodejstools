// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options
{
    public partial class NodejsGeneralOptionsControl : UserControl
    {
        public NodejsGeneralOptionsControl()
        {
            InitializeComponent();
        }

        internal void SyncControlWithPageSettings(NodejsGeneralOptionsPage page)
        {
            this._waitOnAbnormalExit.Checked = page.WaitOnAbnormalExit;
            this._waitOnNormalExit.Checked = page.WaitOnNormalExit;
            this._editAndContinue.Checked = page.EditAndContinue;
        }

        internal void SyncPageWithControlSettings(NodejsGeneralOptionsPage page)
        {
            page.WaitOnAbnormalExit = this._waitOnAbnormalExit.Checked;
            page.WaitOnNormalExit = this._waitOnNormalExit.Checked;
            page.EditAndContinue = this._editAndContinue.Checked;
        }
    }
}
