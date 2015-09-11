using System;

namespace TypeScriptSourceMapReader {
    public struct DkmTextSpan : IComparable<DkmTextSpan>, IEquatable<DkmTextSpan> {
#if SILVERLIGHT
        [SecuritySafeCritical]
#endif
        public int CompareTo(DkmTextSpan other) {
            int ___rv = 0;
            if (this.StartLine != other.StartLine) {
                if (this.StartLine < other.StartLine)
                    return -1;
                else
                    return 1;
            }
            if (this.EndLine != other.EndLine) {
                if (this.EndLine < other.EndLine)
                    return -1;
                else
                    return 1;
            }
            if (this.StartColumn != other.StartColumn) {
                if (this.StartColumn < other.StartColumn)
                    return -1;
                else
                    return 1;
            }
            if (this.EndColumn != other.EndColumn) {
                if (this.EndColumn < other.EndColumn)
                    return -1;
                else
                    return 1;
            }
            return ___rv; // objects equal
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan structure.
        /// </summary>
        /// <param name="other">Value to comare against this instance.</param>
        /// <returns>'true' if the two elements are equal.</returns>
        public bool Equals(DkmTextSpan other) {
            return (this.CompareTo(other) == 0);
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan sructure.
        /// </summary>
        /// <param name="element0">Left side of the comparison</param>
        /// <param name="element1">Right side of the comparison</param>
        public static bool operator !=(DkmTextSpan element0, DkmTextSpan element1) {
            return element0.CompareTo(element1) != 0;
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan sructure.
        /// </summary>
        /// <param name="element0">Left side of the comparison</param>
        /// <param name="element1">Right side of the comparison</param>
        public static bool operator ==(DkmTextSpan element0, DkmTextSpan element1) {
            return element0.CompareTo(element1) == 0;
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan sructure.
        /// </summary>
        /// <param name="element0">Left side of the comparison</param>
        /// <param name="element1">Right side of the comparison</param>
        public static bool operator >(DkmTextSpan element0, DkmTextSpan element1) {
            return element0.CompareTo(element1) > 0;
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan sructure.
        /// </summary>
        /// <param name="element0">Left side of the comparison</param>
        /// <param name="element1">Right side of the comparison</param>
        public static bool operator <(DkmTextSpan element0, DkmTextSpan element1) {
            return element0.CompareTo(element1) < 0;
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan sructure.
        /// </summary>
        /// <param name="element0">Left side of the comparison</param>
        /// <param name="element1">Right side of the comparison</param>
        public static bool operator >=(DkmTextSpan element0, DkmTextSpan element1) {
            return element0.CompareTo(element1) >= 0;
        }

        /// <summary>
        /// Compare two elements of the DkmTextSpan sructure.
        /// </summary>
        /// <param name="element0">Left side of the comparison</param>
        /// <param name="element1">Right side of the comparison</param>
        public static bool operator <=(DkmTextSpan element0, DkmTextSpan element1) {
            return element0.CompareTo(element1) <= 0;
        }
        public override int GetHashCode() {
            int code =
              ((this.StartLine & 0xffff) << 16) |   // bytes 3, 2 are a hash of the start line
              ((this.StartColumn & 0xff) << 8) |    // byte 1 is a hash of the start column
              ((this.EndLine ^ this.EndColumn) % 255);        // byte 0 is a hash of the end line and column
            return code;
        }

        /// <summary>
        /// 1-based integer for the starting source line.
        /// </summary>
        public readonly int StartLine;

        /// <summary>
        /// 1-based integer for the ending source column.
        /// </summary>
        public readonly int EndLine;

        /// <summary>
        /// 1-based integer for the starting source column. If column information is missing
        /// (ex: language service doesn't support it), this value should be set to 0.
        /// </summary>
        public readonly int StartColumn;

        /// <summary>
        /// 1-based integer for the ending source column. If column information is missing
        /// (ex: language service doesn't support it), this value should be set to 0.
        /// </summary>
        public readonly int EndColumn;

        /// <summary>
        /// Initialize a new DkmTextSpan value.
        /// </summary>
        /// <param name="StartLine">
        /// [In] 1-based integer for the starting source line.
        /// </param>
        /// <param name="EndLine">
        /// [In] 1-based integer for the ending source column.
        /// </param>
        /// <param name="StartColumn">
        /// [In] 1-based integer for the starting source column. If column information is
        /// missing (ex: language service doesn't support it), this value should be set to 0.
        /// </param>
        /// <param name="EndColumn">
        /// [In] 1-based integer for the ending source column. If column information is
        /// missing (ex: language service doesn't support it), this value should be set to 0.
        /// </param>
        public DkmTextSpan(
            int StartLine,
            int EndLine,
            int StartColumn,
            int EndColumn
            ) {
            {
            }
            this.StartLine = StartLine;
            this.EndLine = EndLine;
            this.StartColumn = StartColumn;
            this.EndColumn = EndColumn;
        }
    }
}
