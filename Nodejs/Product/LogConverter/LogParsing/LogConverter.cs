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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.NodejsTools.LogGeneration;
using NodeLogConverter;
using NodeLogConverter.LogParsing;

namespace Microsoft.NodejsTools.LogParsing {
    /// <summary>
    /// Parses a V8 profiling log which is just a CSV file.  The CSV file includes several
    /// different record types including tick events with stack traces, code compilation events
    /// with function and line information, and other events which we don't really care about.
    /// 
    /// VS expects to get it's profiling information in the form of an ETL file.  This file is
    /// then zipped up into a VSPX.  
    /// 
    /// VS also expects to take the IP information that is in the file and resolve it against
    /// PDB files.
    /// 
    /// So we do a few different things to translate the CSV file into a form which VS can understand.
    /// 
    /// First, we create a new C# DLL which contains symbol information for the JITed JavaScript functions.
    /// We emit this with #line preprocessor directives so that when a method is resolved to it
    /// it points to the user's JavaScript files.  We rely upon the fact that the C# compiler sequentially
    /// hands out managed method token IDs and we hand out the token IDs in the same order.  We then
    /// open the DLL using the symbol APIs so we can get the checksum on it so VS will happily load
    /// our symbols.
    /// 
    /// Once we've got our DLL produced we can then start processing the log to create the ETL events.
    /// We just run through all of the log entries and spew out the appropriate ETL events.  This includes
    /// the initial process created, module load events, the code creation events, the tick/stack walk
    /// events, and finally the process end event.
    /// 
    /// The log that we create then is a little bit weird - the time stamps have all been provided by
    /// the OS for the current date/time.  On Windows 8 there's a new relogger API which we can use
    /// to re-write the timestamps.  We timed the profiled process, and we know how many ticks we saw,
    /// so we can translate the timestamps to be spread across all of the ticks for the appropiate
    /// amount of time.
    /// 
    /// Finally we zip up the resulting ETL file and save it under the filename requested.
    /// </summary>
    class LogConverter {
        private readonly string _filename;
        private readonly string _outputFile;
        private readonly TimeSpan? _executionTime;
        // Currently we maintain 2 layouts, one for code, one for shared libraries,
        // because V8 is dropping the high 32-bits of code loaded on 64-bit systems.
        // This will let us lookup against the JIT code layout, and then the lib layout, and
        // then the lib layout minus the high 32-bit bits to find the appropriate tag for
        // code and not have to deal with collisions.
        static uint ThreadId = 0x1234;
        static uint ProcessId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
        const int JitModuleId = 1;

        public LogConverter(string filename, string outputFile, TimeSpan? executionTime) {
            _filename = filename;
            _outputFile = outputFile;
            _executionTime = executionTime;
        }

        public static int Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("Usage: <v8.log path> <output.vspx path> [<start time> <execution time>]");
                return 1;
            }
            string inputFile = args[0];
            string outputFile = args[1];
            TimeSpan? executionTime = null;
            if (args.Length > 3) {
                try {
                    //startTime = DateTime.Parse(args[2]);
                    executionTime = TimeSpan.Parse(args[3]);
                } catch {
                    Console.WriteLine("Bad execution time: {0}", executionTime);
                    return 2;
                }
            }

            try {
                var log = new LogConverter(inputFile, outputFile, executionTime);
                log.Process();
                return 0;
            } catch (Exception e) {
                Console.WriteLine("Internal error while converting log: {0}", e.ToString());
                return 2;
            }
        }

        public void Process() {
            string line;

            uint methodToken = 0x06000001;

            string filename = Path.Combine(Path.GetDirectoryName(_outputFile), Path.GetFileNameWithoutExtension(_outputFile));

            Guid pdbSig;
            uint pdbAge;
            string dllPath = CreatePdbFile(out pdbSig, out pdbAge);
            _allMethods.Clear();

            int tickCount = 0;
            using (var reader = new StreamReader(_filename)) {
                var logCreationTime = DateTime.Now;
                var logCreationTimeStamp = Stopwatch.GetTimestamp();

                var log = TraceLog.Start(filename + ".etl", null);

                try {
                    EVENT_TRACE_HEADER header;

                    //////////////////////////////////////
                    header = NewHeader();
                    //header.TimeStamp = currentTimeStamp++;
                    header.Guid = TraceLog.ProcessEventGuid;
                    header.Version = 3;
                    header.Type = 3;

                    var processInfo = new ProcessInfo();
                    processInfo.UniqueProcessKey = 100;
                    processInfo.ProcessId = ProcessId;
                    processInfo.ParentId = 100;

                    log.Trace(header, processInfo, Encoding.ASCII.GetBytes("node.exe"), "");

                    //////////////////////////////////////
                    header = NewHeader();
                    header.Guid = TraceLog.ThreadEventGuid;
                    header.Version = 3;
                    header.Type = 3;

                    var threadInfo = new ThreadInfo();
                    threadInfo.ProcessId = ProcessId;   // setup the thread as if it was a reasonable real thread
                    threadInfo.TThreadId = ThreadId;
                    threadInfo.Affinity = 0xFF;
                    threadInfo.Win32StartAddr = 0x8888;
                    threadInfo.UserStackBase = 0xF0000;
                    threadInfo.UserStackLimit = 0xF00200;
                    threadInfo.IoPriority = 2;
                    log.Trace(header, threadInfo);

                    //////////////////////////////////////
                    header = NewHeader();
                    //header.TimeStamp = currentTimeStamp++;
                    header.Guid = TraceLog.ImageEventGuid;
                    header.Version = 2;
                    header.Type = 3;

                    var imgLoad = new ImageLoad();
                    imgLoad.ImageBase = 0x10000;
                    imgLoad.ImageSize = 10000;
                    imgLoad.ProcessId = ProcessId;
                    log.Trace(header, imgLoad, dllPath);

                    //////////////////////////////////////
                    header = NewHeader();
                    header.Guid = TraceLog.LoaderEventGuid;
                    header.Version = 2;
                    header.Type = 33;

                    var moduleLoad = new ManagedModuleLoadUnload();
                    moduleLoad.AssemblyID = 1;
                    moduleLoad.ModuleID = JitModuleId;

                    log.Trace(header, moduleLoad, dllPath, dllPath,
                        (ushort)1,   // clr instance ID
                        pdbSig,  // pdb sig
                        pdbAge,           // pdb age
                        dllPath.Substring(0, dllPath.Length - 4) + ".pdb",  // path to PDB,
                        Guid.Empty,
                        0,
                        ""
                    );

                    while ((line = reader.ReadLine()) != null) {
                        var records = SplitRecord(line);
                        if (records.Length == 0) {
                            continue;
                        }

                        switch (records[0]) {
                            case "shared-library":
                                if (records.Length < 4) {
                                    continue; // missing info?
                                }
                                break;
                            case "profiler":
                                break;
                            case "code-creation":
                                if (records.Length < 5) {
                                    continue; // missing info?
                                }
                                var startAddr = ParseAddress(records[2]);

                                header = NewMethodEventHeader();

                                var methodLoad = new MethodLoadUnload();
                                methodLoad.MethodID = methodToken;
                                methodLoad.ModuleID = JitModuleId;
                                methodLoad.MethodStartAddress = startAddr;
                                methodLoad.MethodSize = (uint)ParseAddress(records[3]);
                                methodLoad.MethodToken = methodToken;
                                methodLoad.MethodFlags = 8;

                                var funcInfo = ExtractNamespaceAndMethodName(records[4], records[1]);
                                string functionName = funcInfo.Function;
                                if (funcInfo.IsRecompilation) {
                                    functionName += " (recompiled)";
                                }
                                log.Trace(header, methodLoad, funcInfo.Namespace, functionName, "" /* signature*/);

                                methodToken++;
                                break;
                            case "tick":
                                if (records.Length < 5) {
                                    continue;
                                }
                                var addr = ParseAddress(records[1]);

                                header = NewTickRecord();

                                var profileInfo = new SampledProfile();
                                profileInfo.InstructionPointer = (uint)addr;
                                profileInfo.ThreadId = ThreadId;
                                profileInfo.Count = 1;

                                log.Trace(header, profileInfo);

                                if (records.Length > 2) {
                                    header = NewStackWalkRecord();

                                    var sw = new StackWalk();
                                    // this timestamp is independent from the timestamp in our header,
                                    // and this one is not filled in for us by ETW.  So we need to fill
                                    // it in here. 
                                    sw.TimeStamp = (ulong)Stopwatch.GetTimestamp();
                                    sw.Process = ProcessId;
                                    sw.Thread = ThreadId;

                                    // tick, ip, esp, ? [always zero], ?, always zero[?], stack IPs...
                                    const int nonStackFrames = 6;   // count of records, including IP, which aren't stack addresses.

                                    object[] args = new object[2 + records.Length - nonStackFrames];
                                    args[0] = sw;
                                    args[1] = ParseAddress(records[1]); // ip

                                    if ((records.Length - nonStackFrames) == 0) {
                                        // idle CPU time
                                        sw.Process = 0;
                                    }
                                    tickCount++;
                                    for (int i = nonStackFrames; i < records.Length; i++) {
                                        args[i - nonStackFrames + 2] = ParseAddress(records[i]);
                                    }
                                    log.Trace(header, args);
                                }
                                break;
                            default:
                                Console.WriteLine("Unknown record type: {0}", line);
                                break;

                        }
                    }

                    header = NewHeader();
                    header.Guid = TraceLog.ProcessEventGuid;
                    header.Version = 3;
                    header.Type = 4;    // DCEnd

                    processInfo = new ProcessInfo();
                    processInfo.UniqueProcessKey = 100;
                    processInfo.ProcessId = ProcessId;
                    processInfo.ParentId = 100;

                    log.Trace(header, processInfo, Encoding.ASCII.GetBytes("node.exe"), "");
                } finally {
                    log.Stop();
                }

                RelogTrace(_executionTime.Value, tickCount, filename + ".etl");
            }

            // save the VSPX file
            using (var stream = new FileStream(filename + ".vspx", FileMode.Create, FileAccess.ReadWrite, FileShare.None)) {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, false)) {
                    var entry = archive.CreateEntry("VSProfilingData\\" + Path.GetFileName(filename) + ".etl");

                    using (FileStream etlStream = new FileStream(filename + ".etl", FileMode.Open, FileAccess.Read)) {
                        using (var entryStream = entry.Open()) {
                            etlStream.CopyTo(entryStream);
                        }
                    }
                }
            }
        }

        private static ulong ParseAddress(string address) {
            if (address.StartsWith("0x") || address.StartsWith("0X")) {
                return UInt64.Parse(address.Substring(2), NumberStyles.AllowHexSpecifier);
            }
            return UInt64.Parse(address);
        }

        internal static string[] SplitRecord(string line) {
            int start = 0;
            List<string> records = new List<string>();
            bool inQuote = false, containsDoubleQuote = false;
            for (int i = 0; i < line.Length; i++) {
                if (line[i] == ',' && !inQuote) {
                    records.Add(RemoveDoubleQuotes(containsDoubleQuote, line.Substring(start, i - start)));
                    start = i + 1;
                    containsDoubleQuote = false;
                } else if (line[i] == '"') {
                    if (i + 1 == line.Length || line[i + 1] != '"') {
                        inQuote = !inQuote;
                    } else if (inQuote) {
                        containsDoubleQuote = true;
                        i++;
                    }
                }
            }

            if (start < line.Length) {
                var record = RemoveDoubleQuotes(containsDoubleQuote, line.Substring(start, line.Length - start));
                records.Add(record);
            }

            return records.ToArray();
        }

        private static string RemoveDoubleQuotes(bool containsDoubleQuote, string record) {
            if (containsDoubleQuote) {
                record = record.Replace("\"\"", "\"");
            }
            return record;
        }

        internal class FunctionInformation {
            public readonly string Namespace;
            public readonly string Function;
            public readonly string Filename;
            public readonly int? LineNumber;
            public readonly bool IsRecompilation;

            public FunctionInformation(string ns, string methodName, int? lineNo, string filename, bool isRecompilation) {
                Namespace = ns;
                Function = methodName;
                LineNumber = lineNo;
                Filename = filename;
                IsRecompilation = isRecompilation;
            }
        }

        private HashSet<string> _allMethods = new HashSet<string>();

        internal FunctionInformation ExtractNamespaceAndMethodName(string method, string type = "LazyCompile") {
            bool isRecompilation = !_allMethods.Add(method);

            return ExtractNamespaceAndMethodName(method, isRecompilation, type);
        }

        internal static FunctionInformation ExtractNamespaceAndMethodName(string method, bool isRecompilation, string type = "LazyCompile") {
            string methodName = method;
            string ns = "";
            int? lineNo = null;
            string filename = null;

            if (method.StartsWith("\"") && method.EndsWith("\"")) {
                // v8 usually includes quotes, strip them
                method = method.Substring(1, method.Length - 2);
            }

            if (type == "Script" || type == "Function") {
                // code-creation,Function,0xf1c38300,1928," assert.js:1",0xa6d4df58,~
                // code-creation,Script,0xf1c38aa0,244,"assert.js",0xa6d4e050,~
                // code-creation,Script,0xf1c141c0,844,"native runtime.js",0xa6d1aa98,~

                // this is a top level script or module, report it as such
                methodName = "<node module>";
                return new FunctionInformation("<node module>", GetModuleName(method), 1, GetFileName(method), isRecompilation);
            }

            // " net.js:931"
            // "f C:\Source\NodeApp2\NodeApp2\server.js:5"
            // " C:\Source\NodeApp2\NodeApp2\server.js:16"
            int firstSpace = method.IndexOf(' ');
            if (firstSpace != -1) {
                if (firstSpace == 0) {
                    methodName = "anonymous method";
                } else {
                    methodName = method.Substring(0, firstSpace);
                }

                int fileNameEnd = method.LastIndexOf(':');
                if (fileNameEnd != -1 && fileNameEnd > firstSpace) {
                    string lineNumber = method.Substring(fileNameEnd + 1);
                    int lineTemp;
                    if (Int32.TryParse(lineNumber, out lineTemp)) {
                        lineNo = lineTemp;
                    }

                    filename = method.Substring(firstSpace + 1, fileNameEnd - firstSpace - 1);

                    try {
                        var moduleName = Path.GetFileNameWithoutExtension(filename);
                        ns = moduleName;
                    } catch (ArgumentException) {
                        ns = "unknown_module";
                    }
                }
            }

            return new FunctionInformation(ns, methodName, lineNo, filename, isRecompilation);
        }

        private static char[] _invalidPathChars = Path.GetInvalidPathChars();

        private static string GetModuleName(string method) {
            method = StripLine(method);

            if (method.IndexOfAny(_invalidPathChars) == -1) {
                method = Path.GetFileName(method);
            }

            return method;
        }

        private static string GetFileName(string method) {
            method = StripLine(method);

            return method;
        }

        private static string StripLine(string method) {
            method = method.Trim();

            int colon;
            if ((colon = method.LastIndexOf(':')) != -1) {
                string lineNo = method.Substring(colon + 1, method.Length - (colon + 1));
                int dummy;
                // We need to deal with:
                // C:\Foo\Bar\baz.js:1
                //  and
                // C:\Foo\Bar\baz.js
                if (Int32.TryParse(lineNo, out dummy)) {
                    method = method.Substring(0, colon);
                }
            }
            return method;
        }

        private static EVENT_TRACE_HEADER NewMethodEventHeader() {
            var header = NewHeader();
            header.Size = (ushort)(Marshal.SizeOf(typeof(EVENT_TRACE_HEADER)) + Marshal.SizeOf(typeof(MethodLoadUnload)));
            header.Guid = TraceLog.MethodEventGuid;
            header.Version = 0;
            header.Type = 40;
            return header;
        }

        private static EVENT_TRACE_HEADER NewTickRecord() {
            var header = NewHeader();
            header.Size = (ushort)(Marshal.SizeOf(typeof(EVENT_TRACE_HEADER)) + Marshal.SizeOf(typeof(SampledProfile)));
            header.Guid = TraceLog.PerfInfoEventGuid;
            header.Version = 2;
            header.Type = 46;
            return header;
        }

        private static EVENT_TRACE_HEADER NewStackWalkRecord() {
            var header = NewHeader();
            header.Size = (ushort)(Marshal.SizeOf(typeof(EVENT_TRACE_HEADER)) + Marshal.SizeOf(typeof(StackWalk)) + sizeof(int) * 2);
            header.Guid = TraceLog.StackWalkEventGuid;
            header.Version = 2;
            header.Type = 32;
            return header;
        }

        private static EVENT_TRACE_HEADER NewHeader() {
            var header = new EVENT_TRACE_HEADER();
            header.ProcessId = ProcessId;
            header.ThreadId = ThreadId;
            //header.Flags = 0x200;
            //header.HeaderType = 1;

            return header;
        }

        #region PDB Generation

        /// <summary>
        /// Creates a .pdb file for the code creation in the 
        /// </summary>
        /// <returns></returns>
        private string CreatePdbFile(out Guid pdbSig, out uint pdbAge) {
            StringBuilder pdbCode = new StringBuilder();
            pdbCode.Append("class JavaScriptFunctions {");

            int id = 0;
            string dllDirectory;
            for (; ; ) {
                dllDirectory = Path.Combine(Path.GetTempPath(), "JavaScriptFunctions_" + id);
                id++;
                if (!Directory.Exists(dllDirectory)) {
                    try {
                        Directory.CreateDirectory(dllDirectory);
                        break;
                    } catch {
                    }
                }
            }             

            string dllPath = Path.Combine(dllDirectory, "JavaScriptFunctions.dll");
            uint methodToken = 0x06000001;
            using (var reader = new StreamReader(_filename)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    var records = SplitRecord(line);
                    if (records.Length == 0) {
                        continue;
                    }
                    switch (records[0]) {
                        case "code-creation":
                            var funcInfo = ExtractNamespaceAndMethodName(records[4]);
                            if (funcInfo.LineNumber != null && funcInfo.Filename != null) {
                                pdbCode.Append(String.Format(@"
#line {0} ""{1}""
public static void X{2:X}() {{
}}
", funcInfo.LineNumber, funcInfo.Filename, methodToken));
                            } else {
                                // we need to keep outputting methods just to keep tokens lined up.
                                pdbCode.Append(String.Format(@"
public static void X{0:X}() {{
}}
", methodToken));
                            }
                            methodToken++;
                            break;
                    }
                }
            }

            pdbCode.Append("}");

            var compiler = CodeDomProvider.GetCompilerInfo("csharp");
            var parameters = compiler.CreateDefaultCompilerParameters();
            parameters.IncludeDebugInformation = true;
            parameters.OutputAssembly = dllPath;
            parameters.GenerateExecutable = false;
            var res = compiler.CreateProvider().CompileAssemblyFromSource(parameters, pdbCode.ToString());
            if (res.Errors.Count > 0) {
                Console.WriteLine("Error while generating symbols:");
                foreach (var error in res.Errors) {
                    Console.WriteLine("    ", error);
                }
            }

            ReadPdbSigAndAge(dllPath, out pdbSig, out pdbAge);
            return dllPath;
        }

        public static void ReadPdbSigAndAge(string dllPath, out Guid sig, out uint age) {
            sig = default(Guid);
            age = 0;

            var curProcHandle = System.Diagnostics.Process.GetCurrentProcess().Handle;
            IntPtr duplicatedHandle;
            if (!NativeMethods.DuplicateHandle(
                curProcHandle,
                curProcHandle,
                curProcHandle,
                out duplicatedHandle,
                0,
                false,
                DuplicateOptions.DUPLICATE_SAME_ACCESS
            )) {
                return;
            }

            try {
                if (!NativeMethods.SymInitialize(duplicatedHandle, IntPtr.Zero, false)) {
                    return;
                }

                try {
                    ulong moduleAddress = NativeMethods.SymLoadModuleEx(
                        duplicatedHandle,
                        IntPtr.Zero,
                        dllPath,
                        null,
                        0,
                        0,
                        IntPtr.Zero,
                        0
                    );

                    if (moduleAddress == 0) {
                        return;
                    }

                    IMAGEHLP_MODULE64 module64 = new IMAGEHLP_MODULE64();
                    module64.SizeOfStruct = (uint)Marshal.SizeOf(typeof(IMAGEHLP_MODULE64));

                    if (!NativeMethods.SymGetModuleInfo64(duplicatedHandle, moduleAddress, ref module64)) {
                        return;
                    }

                    sig = module64.PdbSig70;
                    age = module64.PdbAge;
                } finally {
                    NativeMethods.SymCleanup(duplicatedHandle);
                }
            } finally {
                NativeMethods.CloseHandle(duplicatedHandle);
            }
        }

        #endregion

        #region Relogging

        private void RelogTrace(TimeSpan executionTime, int stackWalkCount, string filename) {
            // if we're on Win8 or later we want to re-write the timestamps using the relogger
            ITraceRelogger traceRelogger = null;
            try {
                traceRelogger = (ITraceRelogger)Activator.CreateInstance(Type.GetTypeFromCLSID(EtlNativeMethods.CLSID_TraceRelogger));
            } catch(COMException) {
            }

            if (traceRelogger != null) {
                var callback = new TraceReloggerCallback(executionTime, stackWalkCount);
                ulong newTraceHandle;

                traceRelogger.AddLogfileTraceStream(filename, IntPtr.Zero, out newTraceHandle);

                traceRelogger.SetOutputFilename(filename + ".tmp");
                traceRelogger.RegisterCallback(callback);
                traceRelogger.ProcessTrace();

                File.Copy(filename + ".tmp", filename, true);
            } else {
                Console.WriteLine("Failed to create relogger, timestamps will be incorrect");
            }

        }

        class TraceReloggerCallback : ITraceEventCallback {
            private readonly TimeSpan _executionTime;
            private readonly int _stackWalkCount;
            private long _currentTimeStamp;
            private long _ticksPerStack;
            private const int EmptyStackWalkSize = 24;  // size of an empty stack walk record

            public TraceReloggerCallback(TimeSpan executionTime, int stackWalkCount) {
                _currentTimeStamp = Stopwatch.GetTimestamp();
                _executionTime = executionTime;
                _stackWalkCount = stackWalkCount;
            }

            #region ITraceEventCallback Members

            public void OnBeginProcessTrace(ITraceEvent HeaderEvent, ITraceRelogger Relogger) {
                IntPtr recordPtr;
                HeaderEvent.GetEventRecord(out recordPtr);
                EVENT_RECORD record = (EVENT_RECORD)Marshal.PtrToStructure(recordPtr, typeof(EVENT_RECORD));

                EventTrace_Header props = (EventTrace_Header)Marshal.PtrToStructure(record.UserData, typeof(EventTrace_Header));

                var ticksPerMs = props.PerfFreq / 1000;// PerfFreq is frequence in # of ticks per second (same as Stopwatch.Frequency)
                var msPerStack = _executionTime.TotalMilliseconds / _stackWalkCount;
                _ticksPerStack = (long)(ticksPerMs * (ulong)msPerStack);
            }

            public void OnFinalizeProcessTrace(ITraceRelogger Relogger) {
            }

            public void OnEvent(ITraceEvent Event, ITraceRelogger Relogger) {
                IntPtr recordPtr;
                Event.GetEventRecord(out recordPtr);
                EVENT_RECORD record = (EVENT_RECORD)Marshal.PtrToStructure(recordPtr, typeof(EVENT_RECORD));

                if (IsStackWalkRecord(ref record)) {
                    // path the timestamp in the stack walk record.
                    Marshal.WriteInt64(record.UserData, _currentTimeStamp);
                    _currentTimeStamp += _ticksPerStack;
                    if (record.UserDataLength == EmptyStackWalkSize) {
                        // this is an empty stack walk, which means that the CPU is actually idle.
                        // We inject these when writing the log, and then remove them when doing
                        // the rewrite.  This lets us keep the correct timing information but removing
                        // them gets us correct CPU information in the graph.
                        return;
                    }
                }

                var prevTimeStamp = record.EventHeader.TimeStamp;

                var newTimeStamp = _currentTimeStamp;
                Event.SetTimeStamp(ref newTimeStamp);

                if (IsStackWalkRecord(ref record)) {
                }

                Relogger.Inject(Event);
            }


            private static bool IsStackWalkRecord(ref EVENT_RECORD record) {
                return record.EventHeader.ProviderId == TraceLog.StackWalkEventGuid &&
                        record.EventHeader.EventDescriptor.Opcode == 32 &&
                        record.EventHeader.EventDescriptor.Version == 2;
            }

            #endregion
        }

        #endregion
    }
}
