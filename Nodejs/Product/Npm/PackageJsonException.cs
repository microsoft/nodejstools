// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Npm
{
    [Serializable]
    public class PackageJsonException : Exception, ISerializable
    {
        public PackageJsonException() { }
        public PackageJsonException(string message) : base(message) { }
        public PackageJsonException(string message, Exception innerException) : base(message, innerException) { }
        protected PackageJsonException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
