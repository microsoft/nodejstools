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
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace TestAdapterTests
{
    internal class MockRunContext : IRunContext
    {
        public ITestCaseFilterExpression GetTestCaseFilter(IEnumerable<string> supportedProperties, Func<string, TestProperty> propertyProvider)
        {
            throw new NotImplementedException();
        }

        public bool InIsolation
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsBeingDebugged
        {
            get { return false; }
        }

        public bool IsDataCollectionEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool KeepAlive
        {
            get { throw new NotImplementedException(); }
        }

        public string SolutionDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public string TestRunDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public IRunSettings RunSettings
        {
            get { throw new NotImplementedException(); }
        }
    }
}
