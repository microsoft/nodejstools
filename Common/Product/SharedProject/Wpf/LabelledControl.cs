// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudioTools.Wpf
{
    internal sealed class LabelledControl : ContentControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(LabelledControl), new PropertyMetadata());

        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register("HelpText", typeof(string), typeof(LabelledControl), new PropertyMetadata(HelpText_PropertyChanged));

        private static void HelpText_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(HasHelpTextPropertyKey, !string.IsNullOrWhiteSpace(e.NewValue as string));
        }

        public bool HasHelpText
        {
            get { return (bool)GetValue(HasHelpTextProperty); }
            private set { SetValue(HasHelpTextPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey HasHelpTextPropertyKey = DependencyProperty.RegisterReadOnly("HasHelpText", typeof(bool), typeof(LabelledControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HasHelpTextProperty = HasHelpTextPropertyKey.DependencyProperty;
    }
}

