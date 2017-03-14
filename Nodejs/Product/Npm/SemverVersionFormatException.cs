// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Npm
{
    [Serializable]
    public class SemverVersionFormatException : FormatException
    {
        //  I created this class mainly for the purposes of testability. Semver parsing might fail for any
        //  number of reasons with a format exception, which is what I originally used, but since that may
        //  also be thrown by methods called by SemverVersion.Parse, tests can't differentiate correct handling
        //  of bad input versus behaviour that might be a bug.

        public SemverVersionFormatException() { }

        public SemverVersionFormatException(string message) : base(message) { }

        public SemverVersionFormatException(string message, Exception innerException) : base(message, innerException) { }

        protected SemverVersionFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

