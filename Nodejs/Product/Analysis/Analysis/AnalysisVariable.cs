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


namespace Microsoft.NodejsTools.Analysis {
    class AnalysisVariable : IAnalysisVariable {
        private readonly LocationInfo _loc;
        private readonly VariableType _type;

        public AnalysisVariable(VariableType type, LocationInfo location) {
            _loc = location;
            _type = type;
        }

        #region IAnalysisVariable Members

        public LocationInfo Location {
            get { return _loc; }
        }

        public VariableType Type {
            get { return _type; }
        }

        #endregion
    }

}
