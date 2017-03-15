// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using System.Windows;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    internal static class ErrorHelper
    {
        private static Exception _lastNpmNotFoundException;

        public static string GetExceptionDetailsText(Exception e)
        {
            var buff = new StringBuilder();
            var current = e;
            do
            {
                if (buff.Length > 0)
                {
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

        private static Exception GetNpmNotFoundException(Exception source)
        {
            do
            {
                if (source is NpmNotFoundException)
                {
                    return source;
                }
                source = source.InnerException;
            } while (null != source);
            return null;
        }

        public static void ReportNpmNotInstalled(
            Window owner,
            Exception ex)
        {
            var nnfe = GetNpmNotFoundException(ex);
            if (null == nnfe)
            {
                nnfe = ex;
            }
            else
            {
                //  Don't want to keep bombarding user with same message - there's a real danger this popup
                //  could appear quite a lot if changes are made to the filesystem.
                var report = (null == _lastNpmNotFoundException || ex.Message != _lastNpmNotFoundException.Message);

                _lastNpmNotFoundException = nnfe;

                if (!report)
                {
                    return;
                }
            }

            var message = string.Format(CultureInfo.CurrentCulture, Resources.NpmNotInstalledMessageText, nnfe.Message);
            if (null == owner)
            {
                MessageBox.Show(
                    message,
                    Resources.NpmNotInstalledMessageCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    owner,
                    message,
                    Resources.NpmNotInstalledMessageCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
