// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
            var psi = new ProcessStartInfo(args[2], string.Join(" ", args.Skip(3).Select(arg => ProcessOutput.QuoteSingleArgument(arg))));
            psi.UseShellExecute = false;

            var proc = Process.Start(psi);
            File.WriteAllText(args[1], proc.Id.ToString());
            proc.WaitForExit();

            if (args[0] == "both" ||
                (proc.ExitCode == 0 && args[0] == "normal") ||
                (proc.ExitCode != 0 && args[0] == "abnormal"))
            {
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }

            return proc.ExitCode;
        }
    }
}
