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
using System.IO;
using System.Windows.Forms;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Options {
    public partial class NodejsNpmOptionsControl : UserControl {

        private string _npmCachePath;

        public NodejsNpmOptionsControl() {
            InitializeComponent();
        }


        internal void SyncControlWithPageSettings(NodejsNpmOptionsPage page) {
            _showOutputWhenRunningNpm.Checked = page.ShowOutputWindowWhenExecutingNpm;
            _npmCachePath = page.NpmCachePath;
        }

        internal void SyncPageWithControlSettings(NodejsNpmOptionsPage page) {
            page.ShowOutputWindowWhenExecutingNpm = _showOutputWhenRunningNpm.Checked;
        }

        private void ClearCacheButton_Click(object sender, EventArgs e) {
            try {
                Directory.Delete(_npmCachePath, true);                    
                _cacheClearedSuccessfully.Visible = true;
            } catch (DirectoryNotFoundException) {
                // Directory has already been deleted. Do nothing.
                _cacheClearedSuccessfully.Visible = true;
            } catch (IOException exception) {
                // files are in use or path is too long
                MessageBox.Show(
                           string.Format("Cannot clear npm cache. {0}", exception.Message),
                           "Cannot Clear npm Cache",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                       );
            } catch (Exception exception) {
                try {
                    ActivityLog.LogError(SR.ProductName, exception.ToString());
                } catch (InvalidOperationException) {
                    // Activity Log is unavailable.
                }

                MessageBox.Show(
                           string.Format("Cannot clear npm cache. Try manually deleting the directory: {0}", _npmCachePath),
                           "Cannot Clear npm Cache",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                       );
            }
        }
    }
}