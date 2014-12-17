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


namespace TestUtilities.SharedProject {
    /// <summary>
    /// Defines a project type definition, an instance of this gets exported:
    /// 
    /// [Export]
    /// [ProjectExtension(".njsproj")]                            // required
    /// [ProjectTypeGuid("577B58BB-F149-4B31-B005-FC17C8F4CE7C")] // required
    /// [CodeExtension(".js")]                                    // required
    /// [SampleCode("console.log('hi');")]                        // optional
    /// internal static ProjectTypeDefinition ProjectTypeDefinition = new ProjectTypeDefinition();
    /// </summary>
    public sealed class ProjectTypeDefinition {
    }
}
