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

using System.ComponentModel.Composition;
using TestUtilities.SharedProject;

namespace Microsoft.Nodejs.Tests.UI
{
    public sealed class NodejsTestDefintions
    {
        [Export]
        [ProjectExtension(".njsproj")]
        [ProjectTypeGuid("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")]
        [CodeExtension(".js")]
        [SampleCode("console.log('hi');")]
        internal static ProjectTypeDefinition ProjectTypeDefinition = new ProjectTypeDefinition();
    }
}
