// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;

namespace TestUtilities.Mocks
{
    public class MockClassificationType : IClassificationType
    {
        private readonly string _name;
        private readonly List<IClassificationType> _bases;

        public MockClassificationType(string name, IClassificationType[] bases)
        {
            _name = name;
            _bases = new List<IClassificationType>(bases);
        }

        public IEnumerable<IClassificationType> BaseTypes
        {
            get { return _bases; }
        }

        public string Classification
        {
            get { return _name; }
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

        public void AddBaseType(MockClassificationType mockClassificationType)
        {
            _bases.Add(mockClassificationType);
        }
    }
}

