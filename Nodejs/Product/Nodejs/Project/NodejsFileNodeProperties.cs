// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.NodejsTools.TestFrameworks;

namespace Microsoft.NodejsTools.Project
{
    [ComVisible(true)]
    public class NodejsIncludedFileNodeProperties : IncludedFileNodeProperties
    {
        internal NodejsIncludedFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [SRCategory(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [ResourcesDescription(nameof(Resources.TestFrameworkDescription))]
        [TypeConverter(typeof(TestFrameworkStringConverter))]
        public string TestFramework
        {
            get
            {
                return GetProperty(SR.TestFramework, string.Empty);
            }
            set
            {
                SetProperty(SR.TestFramework, value.ToString());
            }
        }
    }

    [ComVisible(true)]
    public class NodejsLinkFileNodeProperties : LinkFileNodeProperties
    {
        internal NodejsLinkFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [SRCategory(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [ResourcesDescription(nameof(Resources.TestFrameworkDescription))]
        [TypeConverter(typeof(TestFrameworkStringConverter))]
        public string TestFramework
        {
            get
            {
                return GetProperty(SR.TestFramework, string.Empty);
            }
            set
            {
                SetProperty(SR.TestFramework, value.ToString());
            }
        }
    }

    /// <summary>
    /// This type converter doesn't really do any conversions, but allows us to provide
    /// a list of standard values for the test framework.
    /// </summary>
    internal class TestFrameworkStringConverter : StringConverter
    {
        public TestFrameworkStringConverter()
        {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var discover = new TestFrameworkDirectories();
            var knownFrameworkList = discover.GetFrameworkNames();
            return new StandardValuesCollection(knownFrameworkList);
        }
    }
}

