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
using System.Text;

namespace Microsoft.NodejsTools {
    internal sealed class AnalysisConstants {
        internal const string NodeModulesFolder = "node_modules";

        /// <summary>
        /// Maximum practical limit for the dependencies analysis.
        /// </summary>
        /// <remarks>
        /// There no practical reasons to go deeper in dependencies analysis.
        /// Number 4 is very practical. Here the examples which hightlight idea.
        /// 
        /// Example 1
        /// 
        /// Level 0 - Large system. Some code should use properties of the object on level 4 here. 
        /// Level 1 - Subsystem - pass data from level 4
        /// Level 2 - Internal dependency for company - Do some work and pass data to level 1
        /// Level 3 - Framework on top of which created Internal dependency.
        /// Level 4 - Dependency of the framework. Objects from here still should be available.
        /// Level 5 - Dependency of the framework provide some usefull primitive which would be used very often on the level of whole system. Hmmm.
        /// 
        /// Example 2 (reason why I could increase to 5)
        /// 
        /// Level 0 - Large system. Some code should use properties of the object on level 4 here. 
        /// Level 1 - Subsystem - pass data from level 4
        /// Level 2 - Internal dependency for company - Wrap access to internal library and perform business logic. Do some work and pass data to level 1
        /// Level 3 - Internal library which wrap access to API.
        /// Level 4 - Http library.
        /// Level 5 - Promise Polyfill.
        ///
        /// All these examples are highly speculative and I specifically try to create such deep level. 
        /// If you develop on windows with such deep level you already close to your limit, your maximum is probably 10. 
        /// </remarks>
        internal const int MaxAnalysisDepthQualty = 4;

        /// <summary>
        /// Maximum practical limit for the dependencies analysis oriented on producing faster results.
        /// </summary>
        internal const int MaxAnalysisDepthFast = 2;
    }
}
