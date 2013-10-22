using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IKeywords : IEnumerable<string>
    {
        int Count { get; }
        string this[ int index ] { get; }
    }
}