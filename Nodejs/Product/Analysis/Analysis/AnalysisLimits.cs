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
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Analysis {
    [Serializable]
    internal class AnalysisLimits {
        public AnalysisLimits() {
            ReturnTypes = 20;
            InstanceMembers = 50;
            DictKeyTypes = 10;
            DictValueTypes = 30;
            IndexTypes = 30;
            AssignedTypes = 100;
            MergedArgumentTypes = 30;
            MaxArrayLiterals = 50;
            MaxObjectLiteralProperties = 50;
            MaxObjectKeysTypes = 5;
            MaxMergeTypes = 5;
            NestedModulesLimit = AnalysisConstants.MaxAnalysisDepthQualty;
        }

        /// <summary>
        /// Creates instance of the <see cref="AnalysisLimits"/> for medium level of Intellisense support.
        /// </summary>
        /// <returns>An <see cref="AnalysisLimits"/> object representing medium level Initellisense settings.</returns>
        public static AnalysisLimits MakeMediumAnalysisLimits() {
            return new AnalysisLimits() {
                NestedModulesLimit = AnalysisConstants.MaxAnalysisDepthFast
            };
        }

        public static AnalysisLimits MakeLowAnalysisLimits() {
            return new AnalysisLimits() {
                ReturnTypes = 1,
                AssignedTypes = 1,
                DictKeyTypes = 1,
                DictValueTypes = 1,
                IndexTypes = 1,
                InstanceMembers = 1,
                NestedModulesLimit = 1
            };
        }

        /// <summary>
        /// The number of types in a return value at which to start combining
        /// similar types.
        /// </summary>
        public int ReturnTypes { get; set; }

        /// <summary>
        /// The number of types in an instance attribute at which to start
        /// combining similar types.
        /// </summary>
        public int InstanceMembers { get; set; }

        /// <summary>
        /// The number of keys in a dictionary at which to start combining
        /// similar types.
        /// </summary>
        public int DictKeyTypes { get; set; }

        /// <summary>
        /// The number of values in a dictionary entry at which to start
        /// combining similar types. Note that this applies to each value in a
        /// dictionary, not to all values at once.
        /// </summary>
        public int DictValueTypes { get; set; }

        /// <summary>
        /// The number of values in a collection at which to start combining
        /// similar types. This does not apply to dictionaries.
        /// </summary>
        public int IndexTypes { get; set; }

        /// <summary>
        /// The number of values in a normal variable at which to start
        /// combining similar types. This is only applied by assignment
        /// analysis.
        /// </summary>
        public int AssignedTypes { get; set; }

        /// <summary>
        /// The number of types that will force us to start combining
        /// types for arguments to functions.  This is only used after
        /// we've exceeded the limits on our cartesian function analysis.
        /// </summary>
        public int MergedArgumentTypes { get; set; }

        /// <summary>
        /// The maximum number of elements an array literal can contain before
        /// simplifying the analysis.
        /// </summary>
        public int MaxArrayLiterals { get; set; }

        /// <summary>
        /// The maximum number of elements an object literal can contain
        /// before simplifying the analysis.
        /// </summary>
        public int MaxObjectLiteralProperties { get; set; }

        public int MaxObjectKeysTypes { get; set; }

        /// <summary>
        /// Gets the maximum number of types which can be merged at once.
        /// </summary>
        public int MaxMergeTypes { get; set; }

        /// <summary>
        /// Gets the maximum levl  of dependency modules which could be analyzed.
        /// </summary>
        public int NestedModulesLimit { get; set; }

        public override bool Equals(object obj) {
            AnalysisLimits other = obj as AnalysisLimits;
            if (other != null) {
                return
                    other.ReturnTypes == ReturnTypes &&
                    other.InstanceMembers == InstanceMembers &&
                    other.DictKeyTypes == DictKeyTypes &&
                    other.DictValueTypes == DictValueTypes &&
                    other.IndexTypes == IndexTypes &&
                    other.AssignedTypes == AssignedTypes &&
                    other.MergedArgumentTypes == MergedArgumentTypes &&
                    other.MaxArrayLiterals == MaxArrayLiterals &&
                    other.MaxObjectLiteralProperties == MaxObjectLiteralProperties &&
                    other.MaxObjectKeysTypes == MaxObjectKeysTypes &&
                    other.MaxMergeTypes == MaxMergeTypes &&
                    other.NestedModulesLimit == NestedModulesLimit;
            }
            return false;
        }

        public override int GetHashCode() {
            return
                ReturnTypes +
                InstanceMembers +
                DictKeyTypes +
                DictValueTypes +
                IndexTypes +
                AssignedTypes +
                MergedArgumentTypes +
                MaxArrayLiterals +
                MaxObjectLiteralProperties +
                MaxObjectKeysTypes +
                MaxMergeTypes +
                NestedModulesLimit;
        }
    }
}
