// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Utilities;

namespace TestUtilities.Mocks
{
    public class MockContentType : IContentType
    {
        private readonly string _name;
        private readonly List<IContentType> _bases;

        public MockContentType(string name, IContentType[] bases)
        {
            _name = name;
            _bases = new List<IContentType>(bases);
        }

        IEnumerable<IContentType> IContentType.BaseTypes
        {
            get { return _bases; }
        }

        public List<IContentType> BaseTypes
        {
            get
            {
                return _bases;
            }
        }

        public bool IsOfType(string type)
        {
            if (type == _name)
            {
                return true;
            }

            foreach (var baseType in BaseTypes)
            {
                if (baseType.IsOfType(type))
                {
                    return true;
                }
            }
            return false;
        }

        public string DisplayName
        {
            get { return _name; }
        }

        public string TypeName
        {
            get { return _name; }
        }
    }
}

