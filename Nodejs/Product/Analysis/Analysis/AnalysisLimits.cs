/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.Win32;

namespace Microsoft.NodejsTools.Analysis {
    public class AnalysisLimits {
        /// <summary>
        /// Returns a new set of limits, set to the defaults for analyzing user
        /// projects.
        /// </summary>
        public static AnalysisLimits GetDefaultLimits() {
            return new AnalysisLimits();
        }

        /// <summary>
        /// Returns a new set of limits, set to the default for analyzing a
        /// standard library.
        /// </summary>
        public static AnalysisLimits GetStandardLibraryLimits() {
            var limits = new AnalysisLimits();
            limits.ReturnTypes = 10;
            limits.InstanceMembers = 5;
            limits.DictKeyTypes = 5;
            limits.DictValueTypes = 20;
            limits.IndexTypes = 5;
            limits.AssignedTypes = 50;
            return limits;
        }

        private const string ReturnTypesId = "ReturnTypes";
        private const string InstanceMembersId = "InstanceMembers";
        private const string DictKeyTypesId = "DictKeyTypes";
        private const string DictValueTypesId = "DictValueTypes";
        private const string IndexTypesId = "IndexTypes";
        private const string AssignedTypesId = "AssignedTypes";

        /// <summary>
        /// Loads a new instance from the specified registry key.
        /// </summary>
        /// <param name="key">
        /// The key to load settings from. Each setting is a DWORD value. If
        /// null, all settings are assumed to be unspecified and the default
        /// values are used.
        /// </param>
        /// <param name="defaultToStdLib">
        /// If True, unspecified settings are taken from the defaults for
        /// standard library analysis. Otherwise, they are taken from the
        /// usual defaults.
        /// </param>
        public static AnalysisLimits LoadFromStorage(RegistryKey key, bool defaultToStdLib = false) {
            var limits = defaultToStdLib ? GetStandardLibraryLimits() : new AnalysisLimits();

            if (key != null) {
                limits.ReturnTypes = (key.GetValue(ReturnTypesId) as int?) ?? limits.ReturnTypes;
                limits.InstanceMembers = (key.GetValue(InstanceMembersId) as int?) ?? limits.InstanceMembers;
                limits.DictKeyTypes = (key.GetValue(DictKeyTypesId) as int?) ?? limits.DictKeyTypes;
                limits.DictValueTypes = (key.GetValue(DictValueTypesId) as int?) ?? limits.DictValueTypes;
                limits.IndexTypes = (key.GetValue(IndexTypesId) as int?) ?? limits.IndexTypes;
                limits.AssignedTypes = (key.GetValue(AssignedTypesId) as int?) ?? limits.AssignedTypes;
            }

            return limits;
        }

        /// <summary>
        /// Saves the current instance's settings to the specified registry key.
        /// </summary>
        public void SaveToStorage(RegistryKey key) {
            key.SetValue(ReturnTypesId, ReturnTypes, RegistryValueKind.DWord);
            key.SetValue(InstanceMembersId, InstanceMembers, RegistryValueKind.DWord);
            key.SetValue(DictKeyTypesId, DictKeyTypes, RegistryValueKind.DWord);
            key.SetValue(DictValueTypesId, DictValueTypes, RegistryValueKind.DWord);
            key.SetValue(IndexTypesId, IndexTypes, RegistryValueKind.DWord);
            key.SetValue(AssignedTypesId, AssignedTypes, RegistryValueKind.DWord);
        }

        public AnalysisLimits() {
            ReturnTypes = 20;
            InstanceMembers = 50;
            DictKeyTypes = 10;
            DictValueTypes = 30;
            IndexTypes = 30;
            AssignedTypes = 100;
        }

        /// <summary>
        /// The maximum number of files which will be used for cross module
        /// analysis.
        /// 
        /// If null, cross module analysis will not be limited. Otherwise, a
        /// value will cause cross module analysis to be disabled after that
        /// number of files have been loaded.
        /// </summary>
        public int? CrossModule { get; set; }

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
    }
}
