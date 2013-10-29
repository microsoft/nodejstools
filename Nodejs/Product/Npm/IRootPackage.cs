namespace Microsoft.NodejsTools.Npm
{
    public interface IRootPackage
    {
        INodeModules Modules { get; }
        IPackageJson PackageJson { get; }
        string Name { get; }
        SemverVersion Version { get; }
    }
}