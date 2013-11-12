using System;
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Npm{
    [Serializable]
    public class SemverVersionFormatException : FormatException, ISerializable{
        //  I created this class mainly for the purposes of testability. Semver parsing might fail for any
        //  number of reasons with a format exception, which is what I originally used, but since that may
        //  also be thrown by methods called by SemverVersion.Parse, tests can't differentiate correct handling
        //  of bad input versus behaviour that might be a bug.

        public SemverVersionFormatException(){}

        public SemverVersionFormatException(string message) : base(message){}

        public SemverVersionFormatException(string message, Exception innerException) : base(message, innerException){}

        protected SemverVersionFormatException(SerializationInfo info, StreamingContext context) : base(info, context){}

        public override void GetObjectData(SerializationInfo info, StreamingContext context){
            base.GetObjectData(info, context);
        }
    }
}