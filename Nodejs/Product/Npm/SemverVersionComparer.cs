//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.NodejsTools.Npm {

    /// <summary>
    /// Performs semver version comparison in accordance with the rules of precedence
    /// set out under point 11 at http://semver.org/.
    /// </summary>
    public class SemverVersionComparer : IComparer<SemverVersion> {
        private bool IsNumericIdentifier(string identifier) {
            for (int index = 0, size = identifier.Length; index < size; ++index) {
                // If any non-digit is detected, or the first character is a leading 0,
                // the identifier is treated as non-numeric
                if (!char.IsDigit(identifier[index])
                    || index == 0 && identifier[index] == '0') {
                    return false;
                }
            }
            return true;
        }

        private int ComparePreRelease(SemverVersion x, SemverVersion y) {
            string xp = x.PreReleaseVersion,
                    yp = y.PreReleaseVersion;

            // Empty pre-release version has higher precedence than a value for pre-release version
            if (string.IsNullOrEmpty(xp)) {
                return string.IsNullOrEmpty(yp) ? 0 : 1;
            }

            // Same as above;
            if (string.IsNullOrEmpty(yp)) {
                return -1;
            }

            // Identifiers are separated by dots
            string[]    xs = xp.Split('.'),
                        ys = yp.Split('.');

            // Compare identifiers individually until a difference is found
            for (int index = 0, size = Math.Min(xs.Length, ys.Length); index < size; ++index) {
                // Figure out which items in each pair of identifiers is numeric - has a profound
                // impace on the subsequent comparison performed.
                bool xn = IsNumericIdentifier(xs[index]),
                     yn = IsNumericIdentifier(ys[index]);
                if (xn) {
                    if (yn) {
                        // Compare numeric identifiers in the expected way
                        var result = int.Parse(xs[index], CultureInfo.InvariantCulture).CompareTo(int.Parse(ys[index], CultureInfo.InvariantCulture));
                        if (0 != result) {
                            return result;
                        }
                    } else {
                        // Numeric identifiers have lower precedence than non-numeric, so y is greater
                        return -1;
                    }
                } else if (yn) {
                    // Numeric identifiers have lower precedence than non-numeric, so x is greater
                    return 1;
                } else {
                    var result = string.Compare(xs[index], ys[index], StringComparison.CurrentCulture);
                    if (0 != result) {
                        return result;
                    }
                }
            }

            // Still the same? More fields in pre-release information indicates higher precedence,
            // otherwise identifiers are the same.

            if (xs.Length == ys.Length) {
                return 0;
            }

            if (xs.Length < ys.Length) {
                return -1;
            }

            return 1;
        }

        public int Compare(SemverVersion x, SemverVersion y) {
            // The version number comparisons are straightforward until
            // we get into comparing pre-release information. In most cases
            // this shouldn't be necessary. Note that build metadata is
            // NOT included in any precedence comparison, and versions that
            // differ only in build metadata should be treated as equivalent.
            var result = x.Major.CompareTo(y.Major);
            if (0 == result) {
                result = x.Minor.CompareTo(y.Minor);
                if (0 == result) {
                    result = x.Patch.CompareTo(y.Patch);
                    if (0 == result) {
                        result = ComparePreRelease(x, y);
                    }
                }
            }
            return result;
        }
    }
}
