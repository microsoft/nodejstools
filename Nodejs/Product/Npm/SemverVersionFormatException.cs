/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Npm {
    [Serializable]
    public class SemverVersionFormatException : FormatException {
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