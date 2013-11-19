using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    [Serializable]
    public class NpmExecutionException : Exception, ISerializable
    {
        public NpmExecutionException(){}
        public NpmExecutionException(string message) : base(message){}
        public NpmExecutionException(string message, Exception innerException) : base(message, innerException){}
        protected NpmExecutionException(SerializationInfo info, StreamingContext context) : base(info, context){}
    }
}
