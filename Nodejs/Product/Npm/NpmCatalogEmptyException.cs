// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Npm
{
    [Serializable]
    public class NpmCatalogEmptyException : NpmExecutionException, ISerializable
    {
        public NpmCatalogEmptyException() { }
        public NpmCatalogEmptyException(string message) : base(message) { }
        public NpmCatalogEmptyException(string message, Exception innerException) : base(message, innerException) { }
        protected NpmCatalogEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

