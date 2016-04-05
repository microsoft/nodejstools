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
            MaxEvents = 5;

            // There no practical reasons to go deeper in dependencies analysis.
            // Number 4 is very practical. Here the examples which hightlight idea.
            // 
            // Example 1
            // 
            // Level 0 - Large system. Some code should use properties of the object on level 4 here. 
            // Level 1 - Subsystem - pass data from level 4
            // Level 2 - Internal dependency for company - Do some work and pass data to level 1
            // Level 3 - Framework on top of which created Internal dependency.
            // Level 4 - Dependency of the framework. Objects from here still should be available.
            // Level 5 - Dependency of the framework provide some usefull primitive which would be used very often on the level of whole system. Hmmm.
            // 
            // Example 2 (reason why I could increase to 5)
            // 
            // Level 0 - Large system. Some code should use properties of the object on level 4 here. 
            // Level 1 - Subsystem - pass data from level 4
            // Level 2 - Internal dependency for company - Wrap access to internal library and perform business logic. Do some work and pass data to level 1
            // Level 3 - Internal library which wrap access to API.
            // Level 4 - Http library.
            // Level 5 - Promise Polyfill.
            //
            // All these examples are highly speculative and I specifically try to create such deep level. 
            // If you develop on windows with such deep level you already close to your limit, your maximum is probably 10. 
            NestedModulesLimit = 4;
        }

        /// <summary>
        /// <see cref="AnalysisLimits"/> for medium level of Intellisense support.
        /// </summary>
        public static AnalysisLimits MediumAnalysisLimits = new AnalysisLimits() {
            // Maximum practical limit for the dependencies analysis oriented on producing faster results.
            NestedModulesLimit = 2
        };

        public static AnalysisLimits LowAnalysisLimit = new AnalysisLimits() {
            ReturnTypes = 1,
            AssignedTypes = 1,
            DictKeyTypes = 1,
            DictValueTypes = 1,
            IndexTypes = 1,
            InstanceMembers = 1,
            NestedModulesLimit = 1
        };

        /// <summary>
        /// Loads a new instance from the specified registry key.
        /// </summary>
        /// <param name="key">
        /// The key to load settings from. If
        /// null, all settings are assumed to be unspecified and the default
        /// values are used.
        /// </param>
        /// <param name="defaults">
        /// The default analysis limits if they're not available in the regkey.
        /// </param>
        public static AnalysisLimits LoadLimitsFromStorage(RegistryKey key, AnalysisLimits defaults) {
            return new AnalysisLimits() {
                ReturnTypes = GetSetting(key, ReturnTypesId) ?? defaults.ReturnTypes,
                InstanceMembers = GetSetting(key, InstanceMembersId) ?? defaults.InstanceMembers,
                DictKeyTypes = GetSetting(key, DictKeyTypesId) ?? defaults.DictKeyTypes,
                DictValueTypes = GetSetting(key, DictValueTypesId) ?? defaults.DictValueTypes,
                IndexTypes = GetSetting(key, IndexTypesId) ?? defaults.IndexTypes,
                AssignedTypes = GetSetting(key, AssignedTypesId) ?? defaults.AssignedTypes,
                NestedModulesLimit = GetSetting(key, NestedModulesLimitId) ?? defaults.NestedModulesLimit
            };
        }

        private static int? GetSetting(RegistryKey key, string setting) {
            if (key != null) {
                return key.GetValue(ReturnTypesId) as int?;
            }
            return null;
        }

        private const string ReturnTypesId = "ReturnTypes";
        private const string InstanceMembersId = "InstanceMembers";
        private const string DictKeyTypesId = "DictKeyTypes";
        private const string DictValueTypesId = "DictValueTypes";
        private const string IndexTypesId = "IndexTypes";
        private const string AssignedTypesId = "AssignedTypes";
        private const string NestedModulesLimitId = "NestedModulesLimit";

        /// <summary>
        /// The number of types in a return value at which to start combining
        /// similar types.
        /// </summary>
        public int ReturnTypes { get; private set; }

        /// <summary>
        /// The number of types in an instance attribute at which to start
        /// combining similar types.
        /// </summary>
        public int InstanceMembers { get; private set; }

        /// <summary>
        /// The number of keys in a dictionary at which to start combining
        /// similar types.
        /// </summary>
        public int DictKeyTypes { get; private set; }

        /// <summary>
        /// The number of values in a dictionary entry at which to start
        /// combining similar types. Note that this applies to each value in a
        /// dictionary, not to all values at once.
        /// </summary>
        public int DictValueTypes { get; private set; }

        /// <summary>
        /// The number of values in a collection at which to start combining
        /// similar types. This does not apply to dictionaries.
        /// </summary>
        public int IndexTypes { get; private set; }

        /// <summary>
        /// The number of values in a normal variable at which to start
        /// combining similar types. This is only applied by assignment
        /// analysis.
        /// </summary>
        public int AssignedTypes { get; private set; }

        /// <summary>
        /// The number of types that will force us to start combining
        /// types for arguments to functions.  This is only used after
        /// we've exceeded the limits on our cartesian function analysis.
        /// </summary>
        public int MergedArgumentTypes { get; private set; }

        /// <summary>
        /// The maximum number of elements an array literal can contain before
        /// simplifying the analysis.
        /// </summary>
        public int MaxArrayLiterals { get; private set; }

        /// <summary>
        /// The maximum number of elements an object literal can contain
        /// before simplifying the analysis.
        /// </summary>
        public int MaxObjectLiteralProperties { get; private set; }

        public int MaxObjectKeysTypes { get; private set; }

        /// <summary>
        /// Gets the maximum number of types which can be merged at once.
        /// </summary>
        public int MaxMergeTypes { get; private set; }

        /// <summary>
        /// Gets the maximum number of events which can be emitted at once.
        /// </summary>
        public int MaxEvents { get; private set; }

        /// <summary>
        /// Gets the maximum level of dependency modules which could be analyzed.
        /// </summary>
        public int NestedModulesLimit { get; private set; }

        /// <summary>
        /// Checks whether relative path exceed the nested module limit.
        /// </summary>
        /// <param name="nestedModulesDepth">Depth of module file which has to be checked for depth limit.</param>
        /// <returns>True if path too deep in nesting tree; false overwise.</returns>
        public bool IsPathExceedNestingLimit(int nestedModulesDepth) {
            return nestedModulesDepth > NestedModulesLimit;
        }

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
