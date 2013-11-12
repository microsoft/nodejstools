using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm{
    public interface IPkgStringArray : IEnumerable<string>{
        int Count { get; }
        string this[int index] { get; }
    }
}