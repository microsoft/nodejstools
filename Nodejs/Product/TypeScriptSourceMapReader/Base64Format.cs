using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceMapper
{   
    /// <summary>
    /// Class that decodes Base64 format
    /// </summary>
    class Base64Format
    {
        private static string encodedValues = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// Decode given Base64 encoded character
        /// </summary>
        /// <param name="inChar">Bas64 encoded character</param>
        /// <returns>Decoded value</returns>
        static public int decodeChar(char inChar)
        {
            return Base64Format.encodedValues.IndexOf(inChar);
        }
    }
}
