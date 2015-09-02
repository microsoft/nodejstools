using System;
using System.IO;
using System.Linq;
using TypeScriptSourceMapReader;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Custom serializer to convert the different types to byte[]
    /// The serialization and deserialization needs to be always in sync
    /// </summary>
    internal class BinarySerializer
    {
        /// <summary>
        /// Memory stream to write data in
        /// </summary>
        private MemoryStream memoryStream;

        /// <summary>
        /// Binary writer to write basic types in binary format
        /// </summary>
        private BinaryWriter binaryWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        internal BinarySerializer()
        {
            this.memoryStream = new MemoryStream();
            this.binaryWriter = new BinaryWriter(memoryStream);
        }

        /// <summary>
        /// Close the serializer to release the internal resources
        /// </summary>
        internal void  Close() 
        {
            this.binaryWriter.Close();
            this.memoryStream.Close();
        }

        /// <summary>
        /// Get the byte array
        /// </summary>
        /// <returns></returns>
        internal byte[] ToArray()
        {
            return this.memoryStream.ToArray();
        }

        internal void Serialize(bool val)
        {
            this.binaryWriter.Write(val);
        }

        internal void Serialize(int val)
        {
            this.binaryWriter.Write(val);
        }

        internal void Serialize(ulong val)
        {
            this.binaryWriter.Write(val);
        }

        internal void Serialize(string val)
        {
            this.binaryWriter.Write(val);
        }

        private void Serialize(string[] stringArray)
        {
            var stringArrayCount = stringArray.Count();
            this.Serialize(stringArrayCount);
            for (var i = 0; i < stringArrayCount; i++)
            {
                this.Serialize(stringArray[i]);
            }
        }

        internal void Serialize(byte[] byteArray)
        {
            this.Serialize(byteArray.Count());
            this.binaryWriter.Write(byteArray);
        }


        internal void Serialize(Guid guid)
        {
            this.Serialize(guid.ToByteArray());
        }
    }
}
