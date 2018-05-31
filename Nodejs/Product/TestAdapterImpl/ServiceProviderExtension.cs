// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider, Type serviceType)
            where T : class
        {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");
            ValidateArg.NotNull(serviceType, "serviceType");

            var serviceInstance = serviceProvider.GetService(serviceType) as T;
            if (serviceInstance == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, serviceType.Name));
            }

            return serviceInstance;
        }
    }
}
