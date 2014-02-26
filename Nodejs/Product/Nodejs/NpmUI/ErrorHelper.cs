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
using System.Text;
using System.Windows;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    internal static class ErrorHelper {

        private const string CaptionNpmNotInstalled = "npm Not Installed";

        private static Exception _lastNpmNotFoundException;

        public static string GetExceptionDetailsText(Exception e) {
            var buff = new StringBuilder();
            var current = e;
            do {
                if (buff.Length > 0) {
                    buff.Append("Caused by:\r\n");
                }
                buff.Append(current.Message);
                buff.Append("\r\n");
#if DEBUG
                buff.Append(current.GetType());
                buff.Append("\r\n");
                buff.Append(current.StackTrace);
#endif
                current = current.InnerException;
            } while (null != current);
            return buff.ToString();
        }

        private static Exception GetNpmNotFoundException(Exception source) {
            do {
                if (source is NpmNotFoundException) {
                    return source;
                }
                source = source.InnerException;
            } while (null != source);
            return null;
        }

        public static void ReportNpmNotInstalled(
            Window owner,
            Exception ex) {
            var nnfe = GetNpmNotFoundException(ex);
            if (null == nnfe) {
                nnfe = ex;
            } else {
                //  Don't want to keep bombarding user with same message - there's a real danger this popup
                //  could appear quite a lot if changes are made to the filesystem.
                bool report = (null == _lastNpmNotFoundException || ex.Message != _lastNpmNotFoundException.Message);

                _lastNpmNotFoundException = nnfe;

                if (!report) {
                    return;
                }
            }

            var message =
                string.Format(@"Could not find npm.cmd. Ensure you have a recent version of node.js installed and have specified the location of node.exe in the project properties, or that it is available on your system PATH.

The following error occurred trying to execute npm.cmd:

{0}", nnfe.Message);

            if (null == owner) {
                MessageBox.Show(
                    message,
                    CaptionNpmNotInstalled,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } else {
                MessageBox.Show(
                    owner,
                    message,
                    CaptionNpmNotInstalled,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }
    }
}
