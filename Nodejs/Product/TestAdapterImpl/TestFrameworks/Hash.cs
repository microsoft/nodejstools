// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal static class Hash
    {
        /// <summary>
        /// Compute the 64bit FNV-1a hash of a stream of bytes
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// and http://www.isthe.com/chongo/tech/comp/fnv/index.html
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <returns>The FNV-1a hash of <paramref name="stream"/></returns>
        internal static byte[] GetFnvHashCode(Stream stream)
        {
            const ulong FnvOffsetBias = 14695981039346656037;

            var buffer = new byte[4096];
            int bytesRead;
            ulong hash = FnvOffsetBias;
            do
            {
                bytesRead = stream.Read(buffer, 0, 4096);
                if (bytesRead > 0)
                {
                    hash = FNVHashCodeCore(hash, buffer, bytesRead);
                }
            } while (bytesRead > 0);

            return UInt64ToBigEndianBytes(hash);
        }

        /// <summary>
        /// Compute the 64bit FNV-1a hash of a sequence of bytes
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// and http://www.isthe.com/chongo/tech/comp/fnv/index.html
        /// </summary>
        /// <param name="data">The sequence of bytes</param>
        /// <returns>The FNV-1a hash of <paramref name="data"/></returns>
        private static ulong FNVHashCodeCore(ulong hash, byte[] data, int length)
        {
            const ulong FnvPrime = 1099511628211;

            for (int i = 0; i < length; i++)
            {
                hash = unchecked((hash ^ data[i]) * FnvPrime);
            }

            return hash;
        }

        private static byte[] UInt64ToBigEndianBytes(ulong value)
        {
            var result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }

            return result;
        }
    }
}
