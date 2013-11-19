using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{

    [Serializable]
    public class NpmNotFoundException : NpmExecutionException, ISerializable
    {
        public NpmNotFoundException(){}
        public NpmNotFoundException(string message) : base(message){}
        public NpmNotFoundException(string message, Exception innerException) : base(message, innerException){}
        protected NpmNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context){}
    }
}
