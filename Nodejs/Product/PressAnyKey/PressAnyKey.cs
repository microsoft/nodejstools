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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.PressAnyKey {
    class Program {
        static int Main(string[] args) {
            if (args.Length < 3) {
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
                (proc.ExitCode != 0 && args[0] == "abnormal")) {
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }

            return proc.ExitCode;
        }
    }
}
