// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.VisualStudioTools;
using WpfCommands = Microsoft.VisualStudioTools.Wpf.Commands;

namespace Microsoft.NodejsTools.Project.ImportWizard
{
    /// <summary>
    /// Interaction logic for ImportWizard.xaml
    /// </summary>
    internal partial class ImportWizard : DialogWindowVersioningWorkaround
    {
        public static readonly RoutedCommand BrowseFolderCommand = new RoutedCommand();
        public static readonly RoutedCommand BrowseOpenFileCommand = new RoutedCommand();
        public static readonly RoutedCommand BrowseSaveFileCommand = new RoutedCommand();

        public ImportSettings ImportSettings
        {
            get { return (ImportSettings)GetValue(ImportSettingsProperty); }
            private set { SetValue(ImportSettingsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ImportSettingsPropertyKey = DependencyProperty.RegisterReadOnly("ImportSettings", typeof(ImportSettings), typeof(ImportWizard), new PropertyMetadata());
        public static readonly DependencyProperty ImportSettingsProperty = ImportSettingsPropertyKey.DependencyProperty;

        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            private set { SetValue(PageCountPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey PageCountPropertyKey = DependencyProperty.RegisterReadOnly("PageCount", typeof(int), typeof(ImportWizard), new PropertyMetadata(0));
        public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNextDefaultPropertyKey = DependencyProperty.RegisterReadOnly("IsNextDefault", typeof(bool), typeof(ImportWizard), new PropertyMetadata(true));
        public static readonly DependencyProperty IsNextDefaultProperty = IsNextDefaultPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsFinishDefaultPropertyKey = DependencyProperty.RegisterReadOnly("IsFinishDefault", typeof(bool), typeof(ImportWizard), new PropertyMetadata(false));
        public static readonly DependencyProperty IsFinishDefaultProperty = IsFinishDefaultPropertyKey.DependencyProperty;

        private CollectionViewSource _pageSequence;

        public ICollectionView PageSequence
        {
            get { return (ICollectionView)GetValue(PageSequenceProperty); }
            private set { SetValue(PageSequencePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey PageSequencePropertyKey = DependencyProperty.RegisterReadOnly("PageSequence", typeof(ICollectionView), typeof(ImportWizard), new PropertyMetadata());
        public static readonly DependencyProperty PageSequenceProperty = PageSequencePropertyKey.DependencyProperty;

        public ImportWizard()
        {
            this.ImportSettings = new ImportSettings();

            this._pageSequence = new CollectionViewSource
            {
                Source = new ObservableCollection<Page>(new Page[] {
                    new StartupPage { DataContext = this.ImportSettings },
                    new FileSourcePage { DataContext = this.ImportSettings },
                    new SaveProjectPage { DataContext = this.ImportSettings }
                })
            };
            this.PageCount = this._pageSequence.View.OfType<object>().Count();

            this.PageSequence = this._pageSequence.View;
            this.PageSequence.CurrentChanged += this.PageSequence_CurrentChanged;
            this.PageSequence.MoveCurrentToFirst();

            this.DataContext = this;

            InitializeComponent();
        }

        private void PageSequence_CurrentChanged(object sender, EventArgs e)
        {
            SetValue(IsNextDefaultPropertyKey, this.PageSequence.CurrentPosition < this.PageCount - 1);
            SetValue(IsFinishDefaultPropertyKey, this.PageSequence.CurrentPosition >= this.PageCount - 1);
        }

        private void Finish_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ImportSettings != null && this.ImportSettings.IsValid;
        }

        private void Finish_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void Cancel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Cancel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void Back_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PageSequence != null && this.PageSequence.CurrentPosition > 0;
        }

        private void Back_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.PageSequence.MoveCurrentToPrevious();
        }

        private void Next_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PageSequence != null && this.PageSequence.CurrentPosition < this.PageCount - 1;
        }

        private void Next_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.PageSequence.MoveCurrentToNext();
        }

        private void Browse_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            WpfCommands.CanExecute(this, sender, e);
        }

        private void Browse_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            WpfCommands.Executed(this, sender, e);
        }
    }
}

