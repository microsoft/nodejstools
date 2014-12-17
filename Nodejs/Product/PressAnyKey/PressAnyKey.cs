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
