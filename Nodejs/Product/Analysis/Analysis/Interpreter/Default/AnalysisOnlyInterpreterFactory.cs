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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    class AnalysisOnlyInterpreterFactory : PythonInterpreterFactoryWithDatabase {
        readonly IEnumerable<string> _actualDatabasePaths;
        readonly PythonTypeDatabase _actualDatabase;

        public AnalysisOnlyInterpreterFactory(Version version, string description = null)
            : base(
                Guid.NewGuid(),
                description ?? string.Format("Python {0} Analyzer", version),
                new InterpreterConfiguration(version),
                false
        ) { }

        public AnalysisOnlyInterpreterFactory(Version version, IEnumerable<string> databasePaths, string description = null)
            : this(version, description) {
            _actualDatabasePaths = databasePaths.ToList();
        }

        public AnalysisOnlyInterpreterFactory(Version version, PythonTypeDatabase database, string description = null)
            : this(version, description) {
            _actualDatabase = database;
        }

        public override PythonTypeDatabase MakeTypeDatabase(string databasePath, bool includeSitePackages = true) {
            if (_actualDatabase != null) {
                return _actualDatabase;
            } else if (_actualDatabasePaths != null) {
                return new PythonTypeDatabase(this, _actualDatabasePaths);
            } else {
                return PythonTypeDatabase.CreateDefaultTypeDatabase(this);
            }
        }
    }
#endif
}
