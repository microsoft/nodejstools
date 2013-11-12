namespace Microsoft.NodejsTools.Npm.SPI{
    internal class PackageProxy : IPackage{
        public INodeModules Modules { get; internal set; }
        public IPackageJson PackageJson { get; internal set; }
        public bool HasPackageJson { get; internal set; }
        public string Name { get; internal set; }
        public SemverVersion Version { get; internal set; }
        public IPerson Author { get; internal set; }
        public string Description { get; internal set; }
        public string Path { get; internal set; }
        public string RequestedVersionRange { get; internal set; }
        public bool IsListedInParentPackageJson { get; internal set; }
        public bool IsMissing { get; internal set; }
        public bool IsDevDependency { get; internal set; }
        public bool IsOptionalDependency { get; internal set; }
        public bool IsBundledDependency { get; internal set; }
        public PackageFlags Flags { get; internal set; }
    }
}