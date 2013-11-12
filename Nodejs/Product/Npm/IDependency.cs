namespace Microsoft.NodejsTools.Npm{
    public interface IDependency{
        string Name { get; }
        IDependencyUrl Url { get; }
        string VersionRangeText { get; }
    }
}