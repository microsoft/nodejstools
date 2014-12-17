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

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
#if INTERACTIVE_WINDOW
    using IReplEvaluator = IInteractiveEngine;
#endif

    /// <summary>
    /// Creates a REPL window.  Implementations should check replId and ensure that it is a REPL window that they requested to be created.  The
    /// replId which will be provided is the same as the ID passed to IReplWindowProvider.CreateReplWindow.  You can receive an ID which has
    /// not been created during the current Visual Studio session if the user exited Visual Studio with the REPL window opened and docked.  Therefore
    /// the replId should contain enough information to re-create the appropriate REPL window.
    /// </summary>
#if INTERACTIVE_WINDOW
    public interface IInteractiveEngineProvider {
#else
    public interface IReplEvaluatorProvider {
#endif
        IReplEvaluator GetEvaluator(string replId);
    }
}
