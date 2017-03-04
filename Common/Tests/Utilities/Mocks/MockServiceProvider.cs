// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace TestUtilities.Mocks
{
    public class MockServiceProvider : IServiceProvider, IServiceContainer
    {
        public readonly Dictionary<Guid, object> Services = new Dictionary<Guid, object>();

        public object GetService(Type serviceType)
        {
            object service;
            Console.WriteLine("MockServiceProvider.GetService({0})", serviceType.Name);
            Services.TryGetValue(serviceType.GUID, out service);
            return service;
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            Services[serviceType.GUID] = callback(this, serviceType);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            Services[serviceType.GUID] = callback(this, serviceType);
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            Services[serviceType.GUID] = serviceInstance;
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            Services[serviceType.GUID] = serviceInstance;
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            Services.Remove(serviceType.GUID);
        }

        public void RemoveService(Type serviceType)
        {
            Services.Remove(serviceType.GUID);
        }
    }
}

