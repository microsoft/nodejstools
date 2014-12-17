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

namespace Microsoft.NodejsTools.Analysis {
#if FULL_VALIDATION || DEBUG
    [Serializable]
    internal class ValidationException : Exception {
        public ValidationException() { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception inner) : base(message, inner) { }
        protected ValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    internal class ChangeCountExceededException : ValidationException {
        public ChangeCountExceededException() { }
        public ChangeCountExceededException(string message) : base(message) { }
        public ChangeCountExceededException(string message, Exception inner) : base(message, inner) { }
        protected ChangeCountExceededException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    static class Validation {
        public static void Assert(bool expression) {
            if (!expression) {
                try {
                    throw new ValidationException();
                } catch (ValidationException ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Assert<T>(bool expression) where T : ValidationException, new() {
            if (!expression) {
                try {
                    throw new T();
                } catch (ValidationException ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Assert(bool expression, string message, params object[] args) {
            if (!expression) {
                try {
                    throw new ValidationException(string.Format(message, args));
                } catch (ValidationException ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
#endif
}
