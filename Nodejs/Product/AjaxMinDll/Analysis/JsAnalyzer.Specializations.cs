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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis {
#if FALSE
    public delegate IAnalysisSet CallDelegate(Node node, AnalysisUnit unit, IAnalysisSet[] args);

    public partial class JsAnalyzer {
        /// <summary>
        /// Replaces a built-in function (specified by module name and function
        /// name) with a customized delegate which provides specific behavior
        /// for handling when that function is called.
        /// </summary>
        /// <remarks>New in 2.0</remarks>
        public void SpecializeFunction(string moduleName, string name, CallDelegate callable, bool mergeOriginalAnalysis = false) {
            SpecializeFunction(moduleName, name, callable, mergeOriginalAnalysis, true);
        }

        /// <summary>
        /// Replaces a built-in function (specified by module name and function
        /// name) with a customized delegate which provides specific behavior
        /// for handling when that function is called.
        /// </summary>
        /// <remarks>New in 2.0</remarks>
        public void SpecializeFunction(string moduleName, string name, string returnType, bool mergeOriginalAnalysis = false) {
            if (returnType.LastIndexOf('.') == -1) {
                throw new ArgumentException(String.Format("Expected module.typename for return type, got '{0}'", returnType));
            }

            SpecializeFunction(moduleName, name, (n, u, a) => u.FindAnalysisValueByName(n, returnType), mergeOriginalAnalysis, true);
        }

        /// <summary>
        /// Replaces a built-in function (specified by module name and function
        /// name) with a customized delegate which provides specific behavior
        /// for handling when that function is called.
        /// 
        /// Currently this just provides a hook when the function is called - it
        /// could be expanded to providing the interpretation of when the
        /// function is called as well.
        /// </summary>
        private void SpecializeFunction(string moduleName, string name, CallDelegate callable, bool mergeOriginalAnalysis, bool save) {
            ModuleReference module;

            int lastDot;
            string realModName = null;
            if (Modules.TryGetValue(moduleName, out module)) {
                IModule mod = module.Module as IModule;
                if (mod != null) {
                    mod.SpecializeFunction(name, callable, mergeOriginalAnalysis);
                    return;
                }
            } else if ((lastDot = moduleName.LastIndexOf('.')) != -1 &&
                Modules.TryGetValue(realModName = moduleName.Substring(0, lastDot), out module)) {

                IModule mod = module.Module as IModule;
                if (mod != null) {
                    mod.SpecializeFunction(moduleName.Substring(lastDot + 1, moduleName.Length - (lastDot + 1)) + "." + name, callable, mergeOriginalAnalysis);
                    return;
                }
            }

            if (save) {
                SaveDelayedSpecialization(moduleName, name, callable, realModName, mergeOriginalAnalysis);
            }
        }

        /// <summary>
        /// Processes any delayed specialization for when a module is added for the 1st time.
        /// </summary>
        /// <param name="moduleName"></param>
        private void DoDelayedSpecialization(string moduleName) {
            lock (_specializationInfo) {
                List<SpecializationInfo> specInfo;
                if (_specializationInfo.TryGetValue(moduleName, out specInfo)) {
                    foreach (var curSpec in specInfo) {
                        SpecializeFunction(curSpec.ModuleName, curSpec.Name, curSpec.Callable, curSpec.SuppressOriginalAnalysis, save: false);
                    }
                }
            }
        }

        private void SaveDelayedSpecialization(string moduleName, string name, CallDelegate callable, string realModName, bool mergeOriginalAnalysis) {
            lock (_specializationInfo) {
                List<SpecializationInfo> specList;
                if (!_specializationInfo.TryGetValue(realModName ?? moduleName, out specList)) {
                    _specializationInfo[realModName ?? moduleName] = specList = new List<SpecializationInfo>();
                }

                specList.Add(new SpecializationInfo(moduleName, name, callable, mergeOriginalAnalysis));
            }
        }

        class SpecializationInfo {
            public readonly string Name, ModuleName;
            public readonly CallDelegate Callable;
            public readonly bool SuppressOriginalAnalysis;

            public SpecializationInfo(string moduleName, string name, CallDelegate callable, bool mergeOriginalAnalysis) {
                ModuleName = moduleName;
                Name = name;
                Callable = callable;
                SuppressOriginalAnalysis = mergeOriginalAnalysis;
            }
        }

        void AddBuiltInSpecializations() {
        }

        IAnalysisSet Nop(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            return AnalysisSet.Empty;
        }
        
        IAnalysisSet ReturnUnionOfInputs(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            return AnalysisSet.UnionAll(args);
        }
    }
#endif
}
