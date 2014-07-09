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
using AnalysisTests;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisDriver {
    class AnalysisDriver {
        private readonly string[] _packages;
        private readonly string _packagePath;
        private readonly Random _random;
        private readonly bool _installMissingPackages, _installAll, _cleanup;
        private readonly INpmController _npmController;
        private readonly TextWriter _logger;
        private readonly int? _dumpMembers;
        private readonly Dictionary<string, RunStats> _baselineStats;
        private readonly bool _lowAnalysis;

        private AnalysisDriver(string[] packages, string packagePath, bool installMissingPackages, bool installAll, bool cleanup, Random random, TextWriter logger, int? dumpMembers, bool lowAnalysis, Dictionary<string, RunStats> baselineStats) {
            _packages = packages;
            _random = random;
            _packagePath = packagePath;
            _installMissingPackages = installMissingPackages;
            _installAll = installAll;
            _cleanup = cleanup;
            _logger = logger;
            _dumpMembers = dumpMembers;
            _lowAnalysis = lowAnalysis;
            _baselineStats = baselineStats;

            if (_logger != null) {
                _logger.WriteLine("Modules,Total Completions,Time (MS),File Count, Working Set (MB), GC Mem (MB),Run Order");
            }

            _npmController = NpmControllerFactory.Create(packagePath);
            _npmController.OutputLogged += NpmControllerOutputLogged;
            _npmController.ErrorLogged += NpmControllerErrorLogged;
            _npmController.Refresh();

            if (_installAll) {
                foreach (var package in _packages) {
                    if (!Directory.Exists(GetInstalledPackagePath(package.Split('@')[0]))) {
                        InstallPackage(package);
                    }
                }
            }
        }

        private void NpmControllerErrorLogged(object sender, NpmLogEventArgs e) {
            Console.WriteLine("ERR: {0}", e.LogText);
        }

        private void NpmControllerOutputLogged(object sender, NpmLogEventArgs e) {
            Console.WriteLine("NPM: {0}", e.LogText);
        }

        private void InstallPackage(string package) {
            var packageInfo = package.Split('@');
            string packageName = packageInfo[0];
            string packageVersion = "*";
            if (packageInfo.Length > 1) {
                packageVersion = packageInfo[1];
            }
            var task = _npmController.CreateNpmCommander().InstallPackageByVersionAsync(
                packageName,
                packageVersion,
                DependencyType.Standard,
                true
            );
            task.Wait();
            if (!task.Result) {
                Console.WriteLine("Failed to install package {0}", package);
                Assert.Fail("Failed to install package {0}", package);
            }
        }

        private string PrepareDirectory(string[] packages) {
            string dirPath = CreateTempDirectoryPath();
            foreach (var package in packages) {
                string packageName = package.Split('@')[0];

                string cachePath = GetInstalledPackagePath(packageName);
                string destPath = Path.Combine(dirPath, "node_modules", packageName);
                if (!Directory.Exists(cachePath)) {
                    if (_installMissingPackages) {
                        Console.WriteLine("Downloading package {0}", package);
                        InstallPackage(package);
                        if (!Directory.Exists(cachePath)) {
                            Assert.Fail("Install reported success but appears to have failed: {0}", package);
                        }
                    } else {
                        Assert.Fail("Not installing missing package: {0}", package);
                    }
                }

                TestData.CopyFiles(cachePath, destPath);
            }
            return dirPath;
        }

        private string GetInstalledPackagePath(string packageName) {
            return Path.Combine(_packagePath, "node_modules", packageName);
        }

        public void RunAll() {
            for (int i = 0; i < _packages.Length; i++) {
                var package = _packages[i];
                Console.Write("Analyzing: {0,-25} ", package);

                var packages = new[] { package };
                RunOne(packages, i + 1);
            }
        }

        private static AnalysisLimits MakeLowAnalysisLimits() {
            return new AnalysisLimits() {
                ReturnTypes = 1,
                AssignedTypes = 1,
                DictKeyTypes = 1,
                DictValueTypes = 1,
                IndexTypes = 1,
                InstanceMembers = 1
            };
        }


        private void RunOne(string[] packages, int runOrder) {
            string testDir = PrepareDirectory(packages);

            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 3; i++) {
                GC.Collect(GC.MaxGeneration);
                GC.WaitForPendingFinalizers();
            }
            var startingGcMem = GC.GetTotalMemory(true);
            var workingSet = Process.GetCurrentProcess().WorkingSet64;
            var startTime = sw.ElapsedMilliseconds;
            sw.Start();
            var analyzer = Analysis.Analyze(testDir, _lowAnalysis ? MakeLowAnalysisLimits() : null);
            sw.Stop();
            var endingGcMem = GC.GetTotalMemory(true);
            var endWorkingSet = Process.GetCurrentProcess().WorkingSet64;

            int completionCount = 0, secondLevelCompletionCount = 0;

            foreach (var module in analyzer.AllModules) {
                if (String.Equals(Path.Combine(testDir, "app.js"), module.FilePath, StringComparison.OrdinalIgnoreCase)) {
                    foreach (var package in packages) {
                        string packageName = package.Split('@')[0];
                        string requireCode = String.Format("require('{0}')", packageName);

                        var moduleMembers = module.Analysis.GetMembersByIndex(requireCode, 0);
                        completionCount += moduleMembers.Count();

                        foreach (var moduleMember in moduleMembers) {
                            var memberCode = requireCode + "." + moduleMember.Completion;

                            secondLevelCompletionCount += module.Analysis.GetMembersByIndex(memberCode, 0).Count();
                        }

                        if (_dumpMembers != null) {
                            DumpMembers(module.Analysis, requireCode, 0);
                        }
                    }

                    break;
                }
            }

            var time = sw.ElapsedMilliseconds - startTime;
            int fileCount = analyzer.AllModules.Count();
            var totalWorkingSet = (endWorkingSet - workingSet) / (1024 * 1024);

            var stats = new RunStats(
                completionCount,
                secondLevelCompletionCount,
                time,
                fileCount,
                totalWorkingSet,
                (endingGcMem - startingGcMem) / (1024 * 1024),
                runOrder
            );

            Log(packages, stats);

            GC.KeepAlive(analyzer);

            if (_cleanup) {
                Directory.Delete(testDir, true);
            } else {
                Console.WriteLine("   Ran in {0}", testDir);
            }
        }

        private void DumpMembers(ModuleAnalysis moduleAnalysis, string memberCode, int depth) {
            var moduleMembers = moduleAnalysis.GetMembersByIndex(memberCode, 0).ToArray();
            Array.Sort(moduleMembers, (x, y) => String.Compare(x.Name, y.Name));

            if (depth < _dumpMembers.Value) {
                foreach (var member in moduleMembers) {
                    Console.WriteLine("    {0} {1}", new string(' ', depth * 4), member.Name);

                    DumpMembers(moduleAnalysis, memberCode + "." + member.Completion, depth + 1);
                }
            }
        }

        private void Log(string[] packages, RunStats stats) {
            string packageId = string.Join(", ", packages);
            if (_logger != null) {
                _logger.WriteLine(stats.ToCsv(packageId));
                _logger.Flush();
            }

            
            RunStats prevStats = null;
            if (_baselineStats != null) {
                if (!_baselineStats.TryGetValue(packageId, out prevStats)) {
                    Console.WriteLine("Didn't find baseline package: {0}", packageId);
                }
            }

            stats.Write(prevStats);
        }

        public void RunCombo(int comboCount, int runOrder) {
            string[] packages;
            if (comboCount == _packages.Length) {
                packages = _packages;
            } else {
                List<string> tempPackages = new List<string>(_packages);
                packages = new string[comboCount];
                for (int i = 0; i < comboCount; i++) {
                    var index = _random.Next(tempPackages.Count);
                    packages[i] = tempPackages[index];
                    tempPackages.RemoveAt(index);
                }
            }

            Console.WriteLine("Analyzing {0} ", String.Join(",", packages));
            Console.Write("        ");
            RunOne(packages, runOrder);
        }

        class RunStats {
            public readonly int TotalCompletions;
            public readonly int SecondLevelCompletions;
            public readonly long Time;
            public readonly int FileCount;
            public readonly long TotalWorkingSet;
            public readonly long GcMem;
            public readonly int RunOrder;

            public RunStats(int totalCompletions, int secondLevelCompletions, long time, int fileCount, long totalWorkingSet, long gcMem, int runOrder) {
                TotalCompletions = totalCompletions;
                SecondLevelCompletions = secondLevelCompletions;
                Time = time;
                FileCount = fileCount;
                TotalWorkingSet = totalWorkingSet;
                GcMem = gcMem;
                RunOrder = runOrder;
            }

            public void Write(RunStats baseline = null) {
                Console.ForegroundColor = ConsoleColor.Gray;
                int left = Console.CursorLeft;
                Console.WriteLine("{0,6} ms {1,4} files {2,4} MB working set, {5,4} MB GC Mem, {3,3} ({4,5}) completions",
                    Time,
                    FileCount,
                    TotalWorkingSet,
                    TotalCompletions,
                    SecondLevelCompletions,
                    GcMem
                );
                if (baseline != null) {
                    Console.CursorLeft = Math.Max(1, left - "DIFF: ".Length);
                    Console.Write("DIFF: ");
                    LogDiff("{0,6} ms ", Time - baseline.Time);
                    LogDiff("{0,4} files ", FileCount - baseline.FileCount);
                    LogDiff("{0,4} MB working set, ", TotalWorkingSet - baseline.TotalWorkingSet);
                    LogDiff("{0,4} MB GC Mem, ", GcMem - baseline.GcMem);
                    LogDiff("{0,3} ", TotalCompletions - baseline.TotalCompletions, reversed: true);
                    LogDiff("({0,5}) completions", SecondLevelCompletions - baseline.SecondLevelCompletions, reversed: true);
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }

            private void LogDiff(string format, long diff, bool reversed = false) {
                if (diff == 0) {
                    Console.ForegroundColor = ConsoleColor.Gray;
                } else if ((!reversed && diff > 0) || (reversed && diff < 0)) {
                    Console.ForegroundColor = ConsoleColor.Red;
                } else {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.Write(format, diff);
            }

            public string ToCsv(string packageName) {
                return String.Format(
                    "\"{0}\",{1},{2},{3},{4},{5},{6},{7}",
                    packageName,
                    TotalCompletions,
                    SecondLevelCompletions,
                    Time,
                    FileCount,
                    TotalWorkingSet,
                    GcMem,
                    RunOrder
                );
            }

            public static KeyValuePair<string, RunStats> Parse(string csv) {
                var columns = new TextFieldParser(new StringReader(csv)) { Delimiters = new [] { "," } }.ReadFields();

                try {
                    return new KeyValuePair<string, RunStats>(
                        columns[0],
                        new RunStats(
                            Int32.Parse(columns[1]), // total completions
                            Int32.Parse(columns[2]), // 2nd level completions
                            Int64.Parse(columns[3]), // time
                            Int32.Parse(columns[4]), // file count
                            Int64.Parse(columns[5]), // working set
                            Int64.Parse(columns[6]), // GC mem
                            Int32.Parse(columns[7])  // run order
                        )
                    );
                } catch (FormatException) {
                    return default(KeyValuePair<string, RunStats>);
                }
            }
        }

        private static int Main(string[] args) {
            List<string> packages = new List<string>();
            if (args.Length == 0) {
                DisplayHelp();
                return 0;
            }

            int? topCount = null, comboCount = null, runCount = null;
            bool combo = false;
            int? seed = null, dumpMembers = null;
            string packagePath = null, logPath = null;
            bool installMissingPackages = false, installAll = false, cleanup = true, lowAnalysis = false;
            Dictionary<string, RunStats> stats = null;
            foreach (var arg in args) {
                if (arg == "/?" || arg == "--help") {
                    DisplayHelp();
                    return 0;
                } else if (arg == "/install_missing") {
                    installMissingPackages = true;
                } else if (arg == "/install_all") {
                    installAll = true;
                } else if (arg == "/low") {
                    lowAnalysis = true;
                } else if (arg == "/no_cleanup") {
                    cleanup = false;
                } else if (arg.StartsWith("/package_list:")) {
                    string filename = arg.Substring("/package_list:".Length);
                    try {
                        packages.AddRange(File.ReadAllLines(filename));
                    } catch (Exception e) {
                        Console.WriteLine("Failed to read package list: {0}", filename);
                        Console.WriteLine(e.Message);
                        return -1;
                    }
                } else if (arg.StartsWith("/package:")) {
                    string packageName = arg.Substring("/package:".Length);
                    if (String.IsNullOrWhiteSpace(packageName)) {
                        Console.WriteLine("Missing package name");
                        return -3;
                    }
                    foreach (var name in packageName.Split(',')) {
                        packages.Add(name.Trim());
                    }
                } else if (arg.StartsWith("/log:")) {
                    logPath = arg.Substring("/log:".Length);
                    if (String.IsNullOrWhiteSpace(logPath)) {
                        Console.WriteLine("Missing log file name");
                        return -16;
                    }
                } else if (arg.StartsWith("/package_path:")) {
                    packagePath = arg.Substring("/package_path:".Length);
                    if (String.IsNullOrWhiteSpace(packagePath)) {
                        Console.WriteLine("Missing package path");
                        return -10;
                    }
                    if (!Directory.Exists(packagePath)) {
                        Console.WriteLine("Package path directory does not exist");
                        return -11;
                    }
                } else if (arg.StartsWith("/seed:")) {
                    int seedInt;
                    if (!Int32.TryParse(arg.Substring("/seed:".Length), out seedInt)) {
                        Console.WriteLine("Bad seed value: {0}", arg.Substring("/seed:".Length));
                        return -8;
                    }
                    seed = seedInt;
                } else if (arg.StartsWith("/combo:")) {
                    string comboArgs = arg.Substring("/combo:".Length);
                    if (String.IsNullOrWhiteSpace(comboArgs)) {
                        Console.WriteLine("Missing combo values");
                        return -4;
                    }
                    var comboArgsArray = comboArgs.Split(',');
                    if (comboArgsArray.Length != 1 && comboArgsArray.Length != 2) {
                        Console.WriteLine("Invalid number of arguments for combination run: {0}", comboArgs);
                        return -6;
                    }
                    int comboArgValue;
                    if (!Int32.TryParse(comboArgsArray[0], out comboArgValue)) {
                        Console.WriteLine("Invalid package count for combination run: {0}", comboArgsArray[0]);
                        return -5;
                    }
                    comboCount = comboArgValue;
                    if (comboArgsArray.Length > 1) {
                        if (!Int32.TryParse(comboArgsArray[1], out comboArgValue)) {
                            Console.WriteLine("Invalid run count for combination run: {0}", comboArgsArray[1]);
                            return -7;
                        }
                        runCount = comboArgValue;
                    }
                } else if (arg.StartsWith("/combo")) {
                    combo = true;
                } else if (arg.StartsWith("/top:")) {
                    int topCountInt;
                    if (!Int32.TryParse(arg.Substring("/top:".Length), out topCountInt)) {
                        Console.WriteLine("Bad top count: {0}", arg.Substring("/top:".Length));
                        return -2;
                    }
                    topCount = topCountInt;
                } else if (arg.StartsWith("/dump_members:")) {
                    int dumpMembersInt;
                    if (!Int32.TryParse(arg.Substring("/dump_members:".Length), out dumpMembersInt)) {
                        Console.WriteLine("Bad dump members count: {0}", arg.Substring("/dump_members:".Length));
                        return -2;
                    }
                    dumpMembers = dumpMembersInt;
                } else if (arg.StartsWith("/dump_members")) {
                    dumpMembers = 1;
                } else if (arg.StartsWith("/baseline:")) {
                    stats = new Dictionary<string, RunStats>();
                    string baselinePath = arg.Substring("/baseline:".Length);
                    if (String.IsNullOrWhiteSpace(baselinePath) || !File.Exists(baselinePath)) {
                        Console.WriteLine("Bad baseline path: {0}", baselinePath);
                        return -20;
                    }
                    var baselineEntries = File.ReadAllLines(baselinePath).Skip(1).ToArray();
                    foreach (var line in baselineEntries) {
                        var kvp = RunStats.Parse(line);
                        if (kvp.Key != null) {
                            stats.Add(kvp.Key, kvp.Value);
                        }
                    }
                } else {
                    Console.WriteLine("Unknown option: {0}", arg);
                    return -15;
                }
            }


            if (packages.Count == 0) {
                Console.WriteLine("No packages were specified");
                return -9;
            }

            if (topCount != null) {
                packages = new List<string>(packages.Where((value, index) => index < topCount.Value));
            }

            if (packagePath == null) {
                packagePath = CreateTempDirectoryPath();
                Console.WriteLine("Packages cached at {0}", packagePath);
                installMissingPackages = true;
            }

            Random random;
            if (seed == null) {
                seed = Environment.TickCount;
                if (comboCount != null) {
                    Console.WriteLine("Seed set to {0}", seed);
                }
            }

            random = new Random(seed.Value);
            TextWriter logger = null;
            if (logPath != null) {
                logger = new StreamWriter(logPath);
            }

            var driver = new AnalysisDriver(
                packages.ToArray(),
                packagePath,
                installMissingPackages,
                installAll,
                cleanup,
                random,
                logger,
                dumpMembers,
                lowAnalysis,
                stats
            );
            if (combo) {
                comboCount = packages.Count;
                runCount = 1;
            }
            try {
                try {
                    if (comboCount == null) {
                        driver.RunAll();
                    } else {
                        for (int i = 0; i < (runCount ?? 1); i++) {
                            driver.RunCombo(comboCount.Value, i + 1);
                        }
                    }
                } finally {
                    if (logger != null) {
                        logger.Close();
                    }
                }
            } catch (AssertFailedException e) {
                Console.WriteLine("Run failed: {0}", e.Message);
                return -100;
            } catch (Exception e) {
                Console.WriteLine("Error during run: {0}\r\n{1}", e.Message, e.StackTrace);
                return -200;
            }
            return 0;
        }

        private static void DisplayHelp() {
            Console.WriteLine("AnalysisDriver [/package_list:<filename>] [/package:<package_name>]");
            Console.WriteLine("               [/combo[:<package_count>[,run_count]] [/seed:<seed>]");
            Console.WriteLine("               [/package_path:<path>] [/install_missing] [/log:<log_file>]");
            Console.WriteLine("               [/dump_members[:level]] [/baseline:<baseline_csv_path>]");
            Console.WriteLine("               [/low]");
            Console.WriteLine();
            Console.WriteLine("At least one package must be specified.  By default all of the packages will be");
            Console.WriteLine("analyzed one by one.  If /combo is provided a packages will be analyzed together");
            Console.WriteLine("with a potential maximum number of packages picked randomly");
            Console.WriteLine();
            Console.WriteLine("By default when running without providing a package path packages will be");
            Console.WriteLine("downloaded and installed to a temporary folder.  When providing package_path");
            Console.WriteLine("they  will not be downloaded unless /install_missing is passed.  It's recommended");
            Console.WriteLine("to use /install_all to force the installation of all packages before the run");
            Console.WriteLine("begins.");
            Console.WriteLine();
            Console.WriteLine(" /package_list:<filename>                 Specifies a list of packages to be read from");
            Console.WriteLine("                                            a filename");
            Console.WriteLine(" /package:<package_name>[,...]            Specifies an one or more packages");
            Console.WriteLine(" /combo[:<package_count>[,<run_count>]]   Specifies to run with multiple packages.");
            Console.WriteLine("                                            package_count: number of packages to");
            Console.WriteLine("                                              randomly select (or run with all if not");
            Console.WriteLine("                                              provided)");
            Console.WriteLine("                                            run_count: number of runs to perform with");
            Console.WriteLine("                                              randomly selected packages");
            Console.WriteLine(" /top:<package_count>                     Only consider the top # of packages (in order");
            Console.WriteLine("                                            provided)");
            Console.WriteLine(" /seed:<seed>                             Run with the specified seed");
            Console.WriteLine(" /package_path:<path>                     Copy packages from specified path.  If omitted");
            Console.WriteLine("                                            packages are saved to a temporary directory");
            Console.WriteLine("                                            and installed on demand");
            Console.WriteLine(" /install_missing                         Install missing packages");
            Console.WriteLine(" /install_all                             Install all packages if not installed before");
            Console.WriteLine("                                            analyzing");
            Console.WriteLine(" /no_cleanup                              Don't delete package run directories");
            Console.WriteLine(" /log:<log file>                          Write results to CSV style log file");
            Console.WriteLine(" /dump_members[:level]                    Dump members of module up to specified level");
            Console.WriteLine(" /baseline:<baseline_csv_path>            Compare results to specified baseline");
            Console.WriteLine(" /low                                     Run with low analysis limits");
        }

        private static string CreateTempDirectoryPath() {
            string dirPath;
            while (Directory.Exists(dirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())) ||
                File.Exists(dirPath)) {
            }
            Directory.CreateDirectory(dirPath);
            File.WriteAllText(Path.Combine(dirPath, "package.json"), "{}");
            File.WriteAllText(Path.Combine(dirPath, "app.js"), "");
            return dirPath;
        }

    }
}
