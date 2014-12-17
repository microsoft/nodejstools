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

namespace Microsoft.NodejsTools.Analysis {
    [Flags]
    public enum GetMemberOptions {
        None,
        /// <summary>
        /// True if advanced members (currently defined as name mangled private members) should be hidden.
        /// </summary>
        HideAdvancedMembers = 0x02,

        /// <summary>
        /// True if the members should include keywords which only show up in a statement context that can 
        /// be completed in the current context in addition to the member list.  
        /// 
        /// Keywords are only included when the request for completions is for all top-level members 
        /// during a call to ModuleAnalysis.GetMembersByIndex or when specifically requesting top 
        /// level members via ModuleAnalysis.GetAllAvailableMembersByIndex.
        /// </summary>
        IncludeStatementKeywords = 0x04,

        /// <summary>
        /// True if the members should include keywords which can show up in an expression context that
        /// can be completed in the current context in addition to the member list.  
        /// 
        /// Keywords are only included when the request for completions is for all top-level members 
        /// during a call to ModuleAnalysis.GetMembersByIndex or when specifically requesting top 
        /// level members via ModuleAnalysis.GetAllAvailableMembersByIndex.
        /// </summary>
        IncludeExpressionKeywords = 0x08,
    }

    internal static class GetMemberOptionsExtensions {
        public static bool HideAdvanced(this GetMemberOptions self) {
            return (self & GetMemberOptions.HideAdvancedMembers) != 0;
        }
        public static bool Keywords(this GetMemberOptions self) {
            return (self & GetMemberOptions.IncludeStatementKeywords  | GetMemberOptions.IncludeExpressionKeywords) != 0;
        }
        public static bool StatementKeywords(this GetMemberOptions self) {
            return (self & GetMemberOptions.IncludeStatementKeywords) != 0;
        }
        public static bool ExpressionKeywords(this GetMemberOptions self) {
            return (self & GetMemberOptions.IncludeExpressionKeywords) != 0;
        }
    }
}
