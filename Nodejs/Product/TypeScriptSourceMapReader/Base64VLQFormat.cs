namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// Class that decodes Base64VLQ format
    /// </summary>
    internal class Base64VLQFormat
    {
        /// <summary>
        /// Decode the Base64VLQ encoded string
        /// </summary>
        /// <param name="digitArray">Array containing Base64VLQ encoded characters</param>
        /// <param name="offset">offset in the array where the string starts, this gets updated for each consumed character</param>
        /// <returns>Decoded value</returns>
        internal static int decode(string digitArray, ref int offset)
        {
            bool moreDigits = true;
            int shiftCount = 0;
            int value = 0;

            for (; moreDigits; offset++)
            {
                // 6 digit number
                int currentByte = Base64Format.decodeChar(digitArray[offset]);

                // If msb is set, we still have more bits to continue
                moreDigits = (currentByte & 32) != 0;

                // least significant 5 bits are the next msbs in the final value.
                value = value | ((currentByte & 31) << shiftCount);
                shiftCount += 5;
            }

            // Least significant bit if 1 represents negative and rest of the msb is actual absolute value
            if ((value & 1) == 0)
            {
                // + number
                value = value >> 1;
            }
            else
            {
                // - number
                value = value >> 1;
                value = -value;
            }

            return value;
        }
    }
}