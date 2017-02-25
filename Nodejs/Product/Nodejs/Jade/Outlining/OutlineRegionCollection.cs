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

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// A collection of outline regions than match specific text buffer version
    /// </summary>
    internal class OutlineRegionCollection : TextRangeCollection<OutlineRegion>, ICloneable
    {
        public int TextBufferVersion { get; internal set; }

        public OutlineRegionCollection(int textBufferVersion)
        {
            this.TextBufferVersion = textBufferVersion;
        }

        #region ICloneable
        public virtual object Clone()
        {
            var clone = new OutlineRegionCollection(this.TextBufferVersion);

            foreach (var item in this)
                clone.Add(item.Clone() as OutlineRegion);

            return clone;
        }
        #endregion
    }
}
