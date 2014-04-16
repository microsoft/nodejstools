using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    interface ITestFramework {
        string DiscoverScript(string testFile, string discoverResultFile);
        string Name { get; }
    }
}
