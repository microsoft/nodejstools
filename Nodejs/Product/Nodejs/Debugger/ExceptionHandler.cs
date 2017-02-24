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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class ExceptionHandler
    {
        private ExceptionHitTreatment _defaultExceptionTreatment = ExceptionHitTreatment.BreakNever;
        private Dictionary<string, ExceptionHitTreatment> _exceptionTreatments;

        public ExceptionHandler()
        {
            _exceptionTreatments = GetDefaultExceptionTreatments();
        }

        public bool BreakOnAllExceptions
        {
            get
            {
                return _defaultExceptionTreatment != ExceptionHitTreatment.BreakNever ||
                       _exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever);
            }
        }

        public bool SetExceptionTreatments(ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments)
        {
            bool updated = false;
            foreach (var exceptionTreatment in exceptionTreatments)
            {
                ExceptionHitTreatment treatmentValue;
                if (!_exceptionTreatments.TryGetValue(exceptionTreatment.Key, out treatmentValue) ||
                    (exceptionTreatment.Value != treatmentValue))
                {
                    _exceptionTreatments[exceptionTreatment.Key] = exceptionTreatment.Value;
                    updated = true;
                }
            }
            return updated;
        }

        public bool ClearExceptionTreatments(ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments)
        {
            bool updated = false;
            foreach (var exceptionTreatment in exceptionTreatments)
            {
                ExceptionHitTreatment treatmentValue;
                if (_exceptionTreatments.TryGetValue(exceptionTreatment.Key, out treatmentValue))
                {
                    _exceptionTreatments.Remove(exceptionTreatment.Key);
                    updated = true;
                }
            }
            return updated;
        }

        public bool ResetExceptionTreatments()
        {
            bool updated = false;
            if (_exceptionTreatments.Values.Any(value => value != _defaultExceptionTreatment))
            {
                _exceptionTreatments = GetDefaultExceptionTreatments();
                updated = true;
            }
            return updated;
        }

        public bool SetDefaultExceptionHitTreatment(ExceptionHitTreatment exceptionTreatment)
        {
            if (_defaultExceptionTreatment != exceptionTreatment)
            {
                _defaultExceptionTreatment = exceptionTreatment;
                return true;
            }
            return false;
        }

        public ExceptionHitTreatment GetExceptionHitTreatment(string exceptionName)
        {
            ExceptionHitTreatment exceptionTreatment;
            if (!_exceptionTreatments.TryGetValue(exceptionName, out exceptionTreatment))
            {
                exceptionTreatment = _defaultExceptionTreatment;
            }
            return exceptionTreatment;
        }

        private Dictionary<string, ExceptionHitTreatment> GetDefaultExceptionTreatments()
        {
            var defaultExceptionTreatments = new Dictionary<string, ExceptionHitTreatment>();

            // Get exception names from in NodePackage.Debugger.cs
            foreach (var attr in System.Attribute.GetCustomAttributes(typeof(NodejsPackage)))
            {
                var debugAttr = attr as ProvideNodeDebugExceptionAttribute;
                if (debugAttr != null && !string.IsNullOrEmpty(debugAttr.ExceptionName))
                {
                    defaultExceptionTreatments[debugAttr.ExceptionName] = ExceptionHitTreatment.BreakNever;
                }
            }

            return defaultExceptionTreatments;
        }
    }
}