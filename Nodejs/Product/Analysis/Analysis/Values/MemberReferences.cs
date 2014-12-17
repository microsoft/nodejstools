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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// A collection of references which are keyd off of project entry.
    /// </summary>
    [Serializable]
    class ReferenceDict : Dictionary<ProjectEntry, ReferenceList> {
        public ReferenceList GetReferences(ProjectEntry project) {
            ReferenceList builtinRef;
            if (!TryGetValue(project, out builtinRef) || builtinRef.Version != project.AnalysisVersion) {
                this[project] = builtinRef = new ReferenceList(project);
            }
            return builtinRef;
        }

        public IEnumerable<LocationInfo> AllReferences {
            get {
                foreach (var keyValue in this) {
                    if (keyValue.Value.References != null) {
                        foreach (var reference in keyValue.Value.References) {
                            yield return reference.GetLocationInfo(keyValue.Key);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A list of references as stored for a single project entry.
    /// </summary>
    [Serializable]
    class ReferenceList : IReferenceable {
        public readonly int Version;
        public readonly ProjectEntry Project;
        public ISet<EncodedSpan> References;

        public ReferenceList(ProjectEntry project) {
            Version = project.AnalysisVersion;
            Project = project;
        }

        public void AddReference(EncodedSpan location) {
            HashSetExtensions.AddValue(ref References, location);
        }

        #region IReferenceable Members

        public IEnumerable<KeyValuePair<ProjectEntry, EncodedSpan>> Definitions {
            get { yield break; }
        }

        IEnumerable<KeyValuePair<ProjectEntry, EncodedSpan>> IReferenceable.References {
            get {
                if (References != null) {
                    foreach (var location in References) {
                        yield return new KeyValuePair<ProjectEntry, EncodedSpan>(Project, location);
                    }
                }
            }
        }

        #endregion
    }
}