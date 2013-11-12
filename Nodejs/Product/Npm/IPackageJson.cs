namespace Microsoft.NodejsTools.Npm{
    public interface IPackageJson{
        string Name { get; }
        SemverVersion Version { get; }
        IScripts Scripts { get; }
        IPerson Author { get; }
        string Description { get; }
        IKeywords Keywords { get; }
        string Homepage { get; }
        IBugs Bugs { get; }
        ILicenses Licenses { get; }
        IFiles Files { get; }
        IMan Man { get; }
        IDependencies Dependencies { get; }
        IDependencies DevDependencies { get; }
        IBundledDependencies BundledDependencies { get; }
        IDependencies OptionalDependencies { get; }
        IDependencies AllDependencies { get; }
    }
}