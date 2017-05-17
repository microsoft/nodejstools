// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using EnvDTE;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockDTEProperty : Property
    {
        private object _value;

        public MockDTEProperty(object value)
        {
            _value = value;
        }

        public object Application
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Properties Collection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DTE DTE
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public short NumIndices
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Object
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Properties Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        public object get_IndexedValue(object Index1, object Index2, object Index3, object Index4)
        {
            throw new NotImplementedException();
        }

        public void let_Value(object lppvReturn)
        {
            throw new NotImplementedException();
        }

        public void set_IndexedValue(object Index1, object Index2, object Index3, object Index4, object Val)
        {
            throw new NotImplementedException();
        }
    }
}

