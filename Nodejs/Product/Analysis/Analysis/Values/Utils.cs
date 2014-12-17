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
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.NodejsTools.Analysis.Values {
    internal static class Utils {
        internal static T[] RemoveFirst<T>(this T[] array) {
            if (array.Length < 1) {
                return new T[0];
            }
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 1, result, 0, array.Length - 1);
            return result;
        }

        internal static string StripDocumentation(string doc) {
            if (doc == null) {
                return String.Empty;
            }
            StringBuilder result = new StringBuilder(doc.Length);
            foreach (string line in doc.Split('\n')) {
                if (result.Length > 0) {
                    result.Append("\r\n");
                }
                result.Append(line.Trim());
            }
            return result.ToString();
        }

        internal static string CleanDocumentation(string doc) {
            int ctr = 0;
            var result = new StringBuilder(doc.Length);
            foreach (char c in doc) {
                if (c == '\r') {
                    // pass
                } else if (c == '\n') {
                    ctr++;
                    if (ctr < 3) {
                        result.Append("\r\n");
                    }
                } else {
                    result.Append(c);
                    ctr = 0;
                }
            }
            return result.ToString().Trim();
        }

        internal static T First<T>(IEnumerable<T> sequence) where T : class {
            if (sequence == null) {
                return null;
            }
            var enumerator = sequence.GetEnumerator();
            if (enumerator == null) {
                return null;
            }
            try {
                if (enumerator.MoveNext()) {
                    return enumerator.Current;
                } else {
                    return null;
                }
            } finally {
                enumerator.Dispose();
            }
        }

        internal static T[] Concat<T>(T firstArg, T[] args) {
            if (args == null) {
                return new[] { firstArg };
            }
            var newArgs = new T[args.Length + 1];
            args.CopyTo(newArgs, 1);
            newArgs[0] = firstArg;
            return newArgs;
        }

        internal static T Peek<T>(this List<T> stack) {
            return stack[stack.Count - 1];
        }

        internal static void Push<T>(this List<T> stack, T value) {
            stack.Add(value);
        }

        internal static T Pop<T>(this List<T> stack) {
            int pos = stack.Count - 1;
            var result = stack[pos];
            stack.RemoveAt(pos);
            return result;
        }
    }

    internal class ReferenceComparer<T> : IEqualityComparer<T> where T : class {
        int IEqualityComparer<T>.GetHashCode(T obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }

        bool IEqualityComparer<T>.Equals(T x, T y) {
            return Object.ReferenceEquals(x, y);
        }
    }
}
