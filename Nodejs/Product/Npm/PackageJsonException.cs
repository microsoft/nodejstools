using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    [ Serializable ]
    public class PackageJsonException : Exception, ISerializable
    {
        public PackageJsonException(){}
        public PackageJsonException(string message) : base(message){}
        public PackageJsonException(string message, Exception innerException) : base(message, innerException){}
        protected PackageJsonException(SerializationInfo info, StreamingContext context) : base(info, context){}
    }
}
