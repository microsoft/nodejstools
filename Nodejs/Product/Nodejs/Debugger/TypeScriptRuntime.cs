using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Script;
using Microsoft.VisualStudio.Debugger.Symbols;
using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Runtime Information
    /// </summary>
    internal static class TypeScriptRuntime
    {
        /// <summary>
        /// Unique id for typescript's custom runtime
        /// </summary>
        internal static Guid TypeScriptRuntimeGuid = new Guid("{89B49315-3387-4431-8095-A39CAFC48BF5}");

        /// <summary>
        /// Unique id for typescript sources
        /// </summary>
        internal static Guid TypeScriptSourceId = new Guid("{55011EC7-5EEE-4A79-884C-2E5DEFA324ED}");

        /// <summary>
        /// Debugger GUID for typescript
        /// </summary>
        internal static Guid TypeScriptDebugLanguageId = new Guid("{87BDF188-E6E8-4FCF-A82A-9B8506E01847}");

        /// <summary>
        /// Compiler id with typescript Debugging engine
        /// </summary>
        internal static DkmCompilerId TypeScriptCompilerId = new DkmCompilerId(DkmVendorId.Microsoft, TypeScriptDebugLanguageId);

        /// <summary>
        /// Script compiler id with ActiveScript debugging engine
        /// </summary>
        internal static DkmCompilerId ScriptCompilerId = new DkmCompilerId(DkmVendorId.Microsoft, DkmLanguageId.Script);

        /// <summary>
        /// Deteremines if runtime is typescript runtime
        /// </summary>
        /// <param name="runtimeInstance"></param>
        /// <returns></returns>
        internal static bool IsTypeScriptRuntime(DkmRuntimeInstance runtimeInstance)
        {
            return runtimeInstance != null && runtimeInstance.Id.RuntimeType == TypeScriptRuntime.TypeScriptRuntimeGuid;
        }

        /// <summary>
        /// Returns true if the module instance is typescript module instance
        /// </summary>
        /// <param name="moduleInstance"></param>
        /// <returns></returns>
        internal static bool IsTypeScriptModuleInstance(DkmModuleInstance moduleInstance)
        {
            return moduleInstance != null && !moduleInstance.IsUnloaded && IsTypeScriptRuntime(moduleInstance.RuntimeInstance);
        }

        /// <summary>
        /// Determines if the module is typescript module by compairing its id
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        internal static bool IsTypeScriptModule(DkmModule module)
        {
            if (module != null)
            {
                var moduleInstances = module.GetModuleInstances();
                return moduleInstances.Length > 0 && IsTypeScriptModuleInstance(moduleInstances[0]);
            }

            return false;
        }

        /// <summary>
        /// Gets the runtime instance for ActiveScript
        /// </summary>
        /// <param name="typeScriptRuntime"></param>
        /// <returns></returns>
        private static DkmRuntimeInstance GetActiveScriptRuntimeInstance(DkmRuntimeInstance typeScriptRuntime)
        {
            Debug.Assert(TypeScriptRuntime.IsTypeScriptRuntime(typeScriptRuntime));
            return typeScriptRuntime.Process.FindRuntimeInstance(new DkmRuntimeInstanceId(DkmRuntimeId.ActiveScript, typeScriptRuntime.Id.InstanceId));
        }

        /// <summary>
        /// Gets the runtime instance for ActiveScriptInterop
        /// </summary>
        /// <param name="typeScriptRuntime"></param>
        /// <returns></returns>
        private static DkmRuntimeInstance GetActiveScriptInteropRuntimeInstance(DkmRuntimeInstance typeScriptRuntime)
        {
            Debug.Assert(TypeScriptRuntime.IsTypeScriptRuntime(typeScriptRuntime));
            return typeScriptRuntime.Process.FindRuntimeInstance(new DkmRuntimeInstanceId(DkmRuntimeId.ActiveScriptInterop, typeScriptRuntime.Id.InstanceId));
        }

        /// <summary>
        /// Gets the script runtime instance from the typescript runtime instance
        /// </summary>
        /// <param name="typeScriptRuntime"></param>
        /// <returns></returns>
        internal static DkmRuntimeInstance GetScriptRuntimeInstance(DkmRuntimeInstance typeScriptRuntime)
        {
            DkmRuntimeInstance jsRuntimeInstance = TypeScriptRuntime.GetActiveScriptRuntimeInstance(typeScriptRuntime);
            if (jsRuntimeInstance == null)
            {
                try
                {
                    // Look if the script interop instance is available
                    jsRuntimeInstance = TypeScriptRuntime.GetActiveScriptInteropRuntimeInstance(typeScriptRuntime);
                }
                catch (MissingMethodException)
                {
                    // On Dev 11 we wont have the ActiveScriptInterop available and 
                    // hence it would throw the missing method exception
                }
            }
            return jsRuntimeInstance;
        }

        /// <summary>
        /// Gets the JS script document with unique id specified and its module instance
        /// </summary>
        /// <param name="typeScriptRuntime">TypeScript runtime instance</param>
        /// <param name="scriptDocumentId">JS document id</param>
        /// <returns>JS script document</returns>
        internal static DkmScriptDocument GetScriptDocument(DkmRuntimeInstance typeScriptRuntime, Guid scriptDocumentId)
        {
            DkmModuleInstance jsModuleInstance;
            return TypeScriptRuntime.GetScriptDocument(typeScriptRuntime, scriptDocumentId, out jsModuleInstance);
        }

        /// <summary>
        /// Gets the JS script document with unique id specified and its module instance
        /// </summary>
        /// <param name="typeScriptRuntime">TypeScript runtime instance</param>
        /// <param name="scriptDocumentId">JS document id</param>
        /// <param name="jsModuleInstance">Returns JS module instance the document belongs to.</param>
        /// <returns>JS script document</returns>
        internal static DkmScriptDocument GetScriptDocument(DkmRuntimeInstance typeScriptRuntime, Guid scriptDocumentId, out DkmModuleInstance jsModuleInstance)
        {
            // Get the JS module instances in the process
            DkmRuntimeInstance jsRuntimeInstance = TypeScriptRuntime.GetScriptRuntimeInstance(typeScriptRuntime);
            var jsModuleInstances = jsRuntimeInstance.GetModuleInstances();

            // Find the script document corresponding to the id specified
            jsModuleInstance = null;
            foreach (var moduleInstance in jsModuleInstances)
            {
                var jsScriptDocument = TypeScriptRuntime.GetScriptDocument(moduleInstance, scriptDocumentId);
                if (jsScriptDocument != null)
                {
                    jsModuleInstance = moduleInstance;
                    return jsScriptDocument;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the script document with the given guid from the module instance
        /// </summary>
        /// <param name="moduleInstance"></param>
        /// <param name="scriptDocumentId"></param>
        /// <returns></returns>
        internal static DkmScriptDocument GetScriptDocument(DkmModuleInstance moduleInstance, Guid scriptDocumentId) {
            var scriptDocuments = moduleInstance.Module.GetScriptDocuments();
            foreach (var scriptDocument in scriptDocuments)
            {
                if (scriptDocument.UniqueId == scriptDocumentId)
                {
                    return scriptDocument;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the Script Language from the process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        internal static DkmLanguage GetScriptLanguage(DkmProcess process)
        {
            var jsLanguage = process.EngineSettings.GetLanguage(TypeScriptRuntime.ScriptCompilerId);
            return jsLanguage;
        }

        /// <summary>
        /// Gets the inspection context for JS from TS
        /// </summary>
        /// <param name="tsInspectionContext"></param>
        /// <returns></returns>
        internal static DkmInspectionContext GetScriptInspectionContext(DkmInspectionContext tsInspectionContext)
        {
            return DkmInspectionContext.Create(tsInspectionContext.InspectionSession,
                 TypeScriptRuntime.GetScriptRuntimeInstance(tsInspectionContext.RuntimeInstance),
                 tsInspectionContext.Thread, tsInspectionContext.Timeout, tsInspectionContext.EvaluationFlags,
                 tsInspectionContext.FuncEvalFlags, tsInspectionContext.Radix,
                 TypeScriptRuntime.GetScriptLanguage(tsInspectionContext.RuntimeInstance.Process),
                 tsInspectionContext.ReturnValue);
        }
    }
}
