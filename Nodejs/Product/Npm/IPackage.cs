namespace Microsoft.NodejsTools.Npm{
    public interface IPackage : IRootPackage{
        string RequestedVersionRange { get; }

        bool IsListedInParentPackageJson { get; }
        bool IsMissing { get; }
        bool IsDevDependency { get; }
        bool IsOptionalDependency { get; }
        bool IsBundledDependency { get; }

        PackageFlags Flags { get; }
    }
}