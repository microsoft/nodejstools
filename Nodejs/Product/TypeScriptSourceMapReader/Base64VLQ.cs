namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// Class that decodes Base64 format
    /// </summary>
    internal class Base64Format
    {
        private static string encodedValues = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// Decode given Base64 encoded character
        /// </summary>
        /// <param name="inChar">Bas64 encoded character</param>
        /// <returns>Decoded value</returns>
        static internal int decodeChar(char inChar)
        {
            return Base64Format.encodedValues.IndexOf(inChar);
        }
    }
}
