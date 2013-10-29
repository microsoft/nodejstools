namespace Microsoft.NodejsTools.Npm
{
    public interface IRootPackage
    {
        INodeModules Modules { get; }
        IPackageJson PackageJson { get; }
        bool HasPackageJson { get; }
        string Name { get; }
        SemverVersion Version { get; }
        string Path { get; }
    }
}