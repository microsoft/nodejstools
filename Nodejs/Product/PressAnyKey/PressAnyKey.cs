// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.PressAnyKey
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: {0} (normal|abnormal|both) (pid file) (path to exe) [args]", Assembly.GetExecutingAssembly().GetName().Name);
            }

            Console.Title = args[2];
            var psi = new ProcessStartInfo(args[2], string.Join(" ", args.Skip(3).Select(arg => ProcessOutput.QuoteSingleArgument(arg))))
            {
                UseShellExecute = false
            };

            var exitCode = 0;
            try
            {
                var proc = Process.Start(psi);
                File.WriteAllText(args[1], proc.Id.ToString());
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Failed to start process.");
                Console.WriteLine("Probable cause is the Node.js exe is corrupt, please re-install.");
                Console.WriteLine($"path: '{args[2]}'.");
                exitCode = -1;
            }

            var shouldWait = true;

            switch (args[0])
            {
                case "both":
                    shouldWait = true;
                    break;
                case "normal":
                    shouldWait = exitCode == 0;
                    break;
                case "abnormal":
                    shouldWait = exitCode != 0;
                    break;
            }

            if (shouldWait)
            {
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }

            return exitCode;
        }
    }
}
