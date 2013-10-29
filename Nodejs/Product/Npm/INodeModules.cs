namespace Microsoft.NodejsTools.Npm
{
    public interface INodeModules
    {
        int Count { get; }
        IPackage this[ int index ] { get; }
        IPackage this[ string name ] { get; }
        bool Contains(string name);
    }
}