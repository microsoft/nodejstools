// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Utilities;

namespace TestUtilities.Mocks
{
    public class MockComponentModel : IComponentModel
    {
        public T GetService<T>() where T : class
        {
            if (typeof(T) == typeof(IErrorProviderFactory))
            {
                return (T)(object)new MockErrorProviderFactory();
            }
            else if (typeof(T) == typeof(IContentTypeRegistryService))
            {
                return (T)(object)new MockContentTypeRegistryService();
            }
            return null;
        }

        public System.ComponentModel.Composition.Primitives.ComposablePartCatalog DefaultCatalog
        {
            get { throw new NotImplementedException(); }
        }

        public System.ComponentModel.Composition.ICompositionService DefaultCompositionService
        {
            get { throw new NotImplementedException(); }
        }

        public System.ComponentModel.Composition.Hosting.ExportProvider DefaultExportProvider
        {
            get { throw new NotImplementedException(); }
        }

        public System.ComponentModel.Composition.Primitives.ComposablePartCatalog GetCatalog(string catalogName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetExtensions<T>() where T : class
        {
            yield break;
        }
    }
}

