// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Jade
{
    internal sealed class ServiceManager : IDisposable
    {
        private const string ServiceManagerId = "ServiceManager";
        private IPropertyOwner _propertyOwner;
        private object _lock = new object();

        private Dictionary<Type, object> _servicesByType = new Dictionary<Type, object>();
        private Dictionary<Guid, object> _servicesByGuid = new Dictionary<Guid, object>();
        private Dictionary<(Type, string), object> _servicesByContentType = new Dictionary<(Type, string), object>();

        private ServiceManager(IPropertyOwner propertyOwner)
        {
            this._propertyOwner = propertyOwner;
            this._propertyOwner.Properties.AddProperty(ServiceManagerId, this);
        }

        /// <summary>
        /// Returns service manager attached to a given Property owner
        /// </summary>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service manager instance</returns>
        public static ServiceManager FromPropertyOwner(IPropertyOwner propertyOwner)
        {
            ServiceManager sm = null;

            if (propertyOwner.Properties.ContainsProperty(ServiceManagerId))
            {
                sm = propertyOwner.Properties.GetProperty(ServiceManagerId) as ServiceManager;
                return sm;
            }

            return new ServiceManager(propertyOwner);
        }

        /// <summary>
        /// Retrieves service from a service manager for this Property owner given service type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service instance</returns>
        public static T GetService<T>(IPropertyOwner propertyOwner) where T : class
        {
            try
            {
                var sm = ServiceManager.FromPropertyOwner(propertyOwner);
                Debug.Assert(sm != null);

                return sm.GetService<T>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves service from a service manager for this property owner given service type GUID.
        /// Primarily used to retrieve services that implement COM interop and are usable from native code.
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="propertyOwner">Property owner</param>
        /// <returns>Service instance</returns>
        public static object GetService(IPropertyOwner propertyOwner, ref Guid serviceGuid)
        {
            try
            {
                var sm = ServiceManager.FromPropertyOwner(propertyOwner);
                Debug.Assert(sm != null);

                return sm.GetService(ref serviceGuid);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///  Retrieves service from a service manager for this Property owner given service type and content type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="propertyOwner">Property owner</param>
        /// <param name="contentType">Content type</param>
        /// <returns>Service instance</returns>
        public static T GetService<T>(IPropertyOwner propertyOwner, IContentType contentType) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            if (sm != null)
            {
                return sm.GetService<T>(contentType);
            }

            return null;
        }

        public static ICollection<T> GetAllServices<T>(IPropertyOwner propertyOwner) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            if (sm != null)
            {
                return sm.GetAllServices<T>();
            }

            return new List<T>();
        }

        /// <summary>
        /// Add service to a service manager associated with a particular Property owner
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        public static void AddService<T>(T serviceInstance, IPropertyOwner propertyOwner) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.AddService<T>(serviceInstance);
        }

        /// <summary>
        /// Add content type specific service to a service manager associated with a particular Property owner
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        /// <param name="contentType">Content type of the service</param>
        public static void AddService<T>(T serviceInstance, IPropertyOwner propertyOwner, IContentType contentType) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.AddService<T>(serviceInstance, contentType);
        }

        /// <summary>
        /// Add service to a service manager associated with a particular property owner.
        /// Typically used to store services implemented in native code and identified by
        /// the interface GUID.
        /// </summary>
        /// <typeparam name="serviceGuid">Service GUID</typeparam>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="propertyOwner">Property owner</param>
        public static void AddService(ref Guid serviceGuid, object serviceInstance, IPropertyOwner propertyOwner)
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.AddService(ref serviceGuid, serviceInstance);
        }

        public static void RemoveService<T>(IPropertyOwner propertyOwner) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.RemoveService<T>();
        }

        public static void RemoveService<T>(IPropertyOwner propertyOwner, IContentType contentType) where T : class
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.RemoveService<T>(contentType);
        }

        public static void RemoveService(IPropertyOwner propertyOwner, ref Guid guidService)
        {
            var sm = ServiceManager.FromPropertyOwner(propertyOwner);
            Debug.Assert(sm != null);

            sm.RemoveService(ref guidService);
        }

        private T GetService<T>() where T : class
        {
            lock (this._lock)
            {

                if (!this._servicesByType.TryGetValue(typeof(T), out var service))
                {
                    // try walk through and cast. Perhaps someone is asking for IFoo
                    // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                    foreach (var kvp in this._servicesByType)
                    {
                        service = kvp.Value as T;
                        if (service != null)
                        {
                            break;
                        }
                    }
                }

                return service as T;
            }
        }

        private T GetService<T>(IContentType contentType) where T : class
        {
            lock (this._lock)
            {

                this._servicesByContentType.TryGetValue((typeof(T), contentType.TypeName), out var service);
                if (service != null)
                {
                    return service as T;
                }

                // Try walking through and cast. Perhaps someone is asking for IFoo
                // that is implemented on class Bar but Bar was added as Bar, not as IFoo
                foreach (var kvp in this._servicesByContentType)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(kvp.Key.Item2, contentType.TypeName))
                    {
                        service = kvp.Value as T;
                        if (service != null)
                        {
                            return service as T;
                        }
                    }
                }

                // iterate through base types since Razor, PHP and ASP.NET content type derive from HTML
                foreach (var ct in contentType.BaseTypes)
                {
                    service = GetService<T>(ct);
                    if (service != null)
                    {
                        break;
                    }
                }

                return service as T;
            }
        }

        private object GetService(ref Guid serviceGuid)
        {
            lock (this._lock)
            {
                foreach (var kvp in this._servicesByGuid)
                {
                    if (serviceGuid.Equals(kvp.Key))
                    {
                        return kvp.Value;
                    }
                }

                foreach (var kvp in this._servicesByType)
                {
                    if (serviceGuid.Equals(kvp.Value.GetType().GUID))
                    {
                        return kvp.Value;
                    }
                }

                return null;
            }
        }

        private ICollection<T> GetAllServices<T>() where T : class
        {
            var list = new List<T>();

            lock (this._lock)
            {
                foreach (var kvp in this._servicesByType)
                {
                    if (kvp.Value is T service)
                    {
                        list.Add(service);
                    }
                }
            }

            return list;
        }

        private void AddService<T>(T serviceInstance) where T : class
        {
            lock (this._lock)
            {
                if (GetService<T>() == null)
                {
                    this._servicesByType.Add(typeof(T), serviceInstance);
                }
            }
        }

        private void AddService<T>(T serviceInstance, IContentType contentType) where T : class
        {
            lock (this._lock)
            {
                if (GetService<T>(contentType) == null)
                {
                    this._servicesByContentType.Add((typeof(T), contentType.TypeName), serviceInstance);
                }
            }
        }

        private void AddService(ref Guid serviceGuid, object serviceInstance)
        {
            lock (this._lock)
            {
                if (GetService(ref serviceGuid) == null)
                {
                    this._servicesByGuid.Add(serviceGuid, serviceInstance);
                }
            }
        }

        private void RemoveService<T>() where T : class
        {
            this._servicesByType.Remove(typeof(T));
        }

        private void RemoveService<T>(IContentType contentType) where T : class
        {
            lock (this._lock)
            {
                this._servicesByContentType.Remove((typeof(T), contentType.TypeName));
            }
        }

        private void RemoveService(ref Guid guidService)
        {
            this._servicesByGuid.Remove(guidService);
        }

        public void Dispose()
        {
            if (this._propertyOwner != null)
            {
                this._propertyOwner.Properties.RemoveProperty(ServiceManagerId);

                this._servicesByGuid.Clear();
                this._servicesByType.Clear();
                this._servicesByContentType.Clear();

                this._propertyOwner = null;
            }
        }
    }
}
