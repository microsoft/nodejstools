// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows;

namespace Microsoft.VisualStudioTools.Wpf
{
    public static class LambdaProperties
    {
        public static readonly DependencyProperty ImportedNamespacesProperty = DependencyProperty.RegisterAttached(
            "ImportedNamespaces", typeof(string), typeof(LambdaProperties));

        public static string GetImportedNamespaces(object obj)
        {
            return null;
        }

        public static void SetImportedNamespaces(object obj, string value)
        {
        }
    }
}

