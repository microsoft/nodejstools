// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using System.Windows.Data;
using System.Globalization;
using Microsoft.VisualStudioTools.Wpf;

namespace Microsoft.VisualStudioTools.Wpf
{
    [ContentProperty("Lambda")]
    public class LambdaExtension : MarkupExtension
    {
        public string Lambda { get; set; }

        public LambdaExtension()
        {
        }

        public LambdaExtension(string lambda)
        {
            this.Lambda = lambda;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.Lambda == null)
            {
                throw new InvalidOperationException("Lambda not specified");
            }

            var rootProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
            var root = rootProvider.RootObject;
            if (root == null)
            {
                throw new InvalidOperationException("Cannot locate root object - service provider did not provide IRootObjectProvider");
            }

            var provider = root as ILambdaConverterProvider;
            if (provider == null)
            {
                throw new InvalidOperationException("Root object does not implement ILambdaConverterProvider - code generator not run");
            }

            return provider.GetConverterForLambda(this.Lambda);
        }
    }
}

