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
using Microsoft.VisualStudioTools.VSTestHost;

namespace Microsoft.Nodejs.Tests.UI {
    class OptionHolder : IDisposable {
        private readonly string _category, _page, _option;
        private readonly object _oldValue;

        public OptionHolder(string category, string page, string option, object newValue) {
            _category = category;
            _page = page;
            _option = option;
            var props = VSTestContext.DTE.get_Properties(category, page);
            _oldValue = props.Item(option).Value;
            props.Item(option).Value = newValue;
        }

        public void Dispose() {
            var props = VSTestContext.DTE.get_Properties(_category, _page);
            props.Item(_option).Value = _oldValue;
        }
    }
}
