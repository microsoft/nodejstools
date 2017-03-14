// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace TestUtilities.Mocks
{
    [Export(typeof(IClassificationTypeRegistryService))]
    public class MockClassificationTypeRegistryService : IClassificationTypeRegistryService
    {
        private static Dictionary<string, MockClassificationType> _types = new Dictionary<string, MockClassificationType>();

        public MockClassificationTypeRegistryService()
        {
            foreach (FieldInfo fi in typeof(PredefinedClassificationTypeNames).GetFields())
            {
                string name = (string)fi.GetValue(null);
                _types[name] = new MockClassificationType(name, new IClassificationType[0]);
            }
        }

        [ImportingConstructor]
        public MockClassificationTypeRegistryService([ImportMany]IEnumerable<Lazy<ClassificationTypeDefinition, IClassificationTypeDefinitionMetadata>> classTypeDefs)
            : this()
        {
            foreach (var def in classTypeDefs)
            {
                string name = def.Metadata.Name;
                var type = GetClasificationType(name);
                foreach (var baseType in def.Metadata.BaseDefinition ?? Enumerable.Empty<string>())
                {
                    type.AddBaseType(GetClasificationType(baseType));
                }
            }
        }

        private static MockClassificationType GetClasificationType(string name)
        {
            MockClassificationType type;
            if (!_types.TryGetValue(name, out type))
            {
                _types[name] = type = new MockClassificationType(name, new IClassificationType[0]);
            }
            return type;
        }

        public IClassificationType CreateClassificationType(string type, IEnumerable<IClassificationType> baseTypes)
        {
            return _types[type] = new MockClassificationType(type, baseTypes.ToArray());
        }

        public IClassificationType CreateTransientClassificationType(params IClassificationType[] baseTypes)
        {
            return new MockClassificationType(String.Empty, baseTypes);
        }

        public IClassificationType CreateTransientClassificationType(IEnumerable<IClassificationType> baseTypes)
        {
            return new MockClassificationType(String.Empty, baseTypes.ToArray());
        }

        public IClassificationType GetClassificationType(string type)
        {
            MockClassificationType result;
            return _types.TryGetValue(type, out result) ? result : null;
        }
    }
}

