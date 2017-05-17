// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.VisualStudioTools.VSTestHost;

namespace Microsoft.Nodejs.Tests.UI
{
    internal class OptionHolder : IDisposable
    {
        private readonly string _category, _page, _option;
        private readonly object _oldValue;

        public OptionHolder(string category, string page, string option, object newValue)
        {
            _category = category;
            _page = page;
            _option = option;
            var props = VSTestContext.DTE.get_Properties(category, page);
            _oldValue = props.Item(option).Value;
            props.Item(option).Value = newValue;
        }

        public void Dispose()
        {
            var props = VSTestContext.DTE.get_Properties(_category, _page);
            props.Item(_option).Value = _oldValue;
        }
    }

    internal class NodejsOptionHolder : IDisposable
    {
        private object _oldValue;
        private PropertyInfo _property;
        private object _page;

        public NodejsOptionHolder(object optionsPage, string propertyName, object newValue)
        {
            _page = optionsPage;
            _property = optionsPage.GetType().GetProperty(propertyName);
            _oldValue = _property.GetValue(_page);
            _property.SetValue(_page, newValue);
        }

        public void Dispose()
        {
            _property.SetValue(_page, _oldValue);
        }
    }
}

