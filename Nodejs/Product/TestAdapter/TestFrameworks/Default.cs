using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class Default : ITestFramework {
        private string _discoverScript = 
            "var fs = require('fs');" + 
            "var stream = fs.createWriteStream('{0}'); " + 
            "var testCase = require('{1}');" + 
            "for(var x in testCase) { stream.write(x + '\\r\\n'); }" + 
            "stream.end();";

        public string Name {
            get {
                return "default";
            }
        }
        public string DiscoverScript (string testFile, string discoverResultFile) {
            return string.Format(_discoverScript, discoverResultFile.Replace("\\", "\\\\"), testFile.Replace("\\", "\\\\"));
        }


    }
}
