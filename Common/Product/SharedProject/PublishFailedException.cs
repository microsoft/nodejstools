// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudioTools.Project
{
    [Serializable]
    public sealed class PublishFailedException : Exception
    {
        public PublishFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private PublishFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

