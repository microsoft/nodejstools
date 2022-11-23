// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Profiling
{
    /// <summary>
    /// Interaction logic for LaunchProfiling.xaml
    /// </summary>
    public partial class LaunchProfiling : DialogWindowVersioningWorkaround
    {
        private readonly ProfilingTargetView _viewModel;

        public LaunchProfiling(ProfilingTargetView viewModel)
        {
            _viewModel = viewModel;

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void FindInterpreterClick(object sender, RoutedEventArgs e)
        {
            var standalone = _viewModel.Standalone;
            if (standalone != null)
            {
                var dlg = new OpenFileDialog();
                // TODO: Specify an OpenFileDialog filter for finding an interpreter to profile
                dlg.CheckFileExists = true;
                bool res = dlg.ShowDialog() ?? false;
                if (res)
                {
                    standalone.InterpreterPath = dlg.FileName;
                }
            }
        }

        private void FindScriptClick(object sender, RoutedEventArgs e)
        {
            var standalone = _viewModel.Standalone;
            if (standalone != null)
            {
                var dlg = new OpenFileDialog();
                // TODO: Specify an OpenFileDialog filter for finding a script to profile
                dlg.CheckFileExists = true;
                bool res = dlg.ShowDialog() ?? false;
                if (res)
                {
                    standalone.ScriptPath = dlg.FileName;
                }
            }
        }

        private void FindWorkingDirectoryClick(object sender, RoutedEventArgs e)
        {
            var standalone = _viewModel.Standalone;
            if (standalone != null)
            {
                var dlg = new System.Windows.Forms.FolderBrowserDialog();
                dlg.SelectedPath = standalone.WorkingDirectory;
                var res = dlg.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    standalone.WorkingDirectory = dlg.SelectedPath;
                }
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}

