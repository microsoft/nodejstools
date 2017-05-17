// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


using System;
using System.Collections;
using System.Collections.Generic;
using EnvDTE;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockDTEProperties : EnvDTE.Properties
    {
        private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();

        public MockDTEProperties()
        {
        }

        public object Application
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                return _properties.Count;
            }
        }

        public DTE DTE
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Property Item(object index)
        {
            return _properties[(string)index];
        }

        public void Add(string name, object value)
        {
            _properties.Add(name, new MockDTEProperty(value));
        }
    }
}

