namespace Microsoft.NodejsTools.Npm
{
    public interface IDependencyUrl
    {
        string Address { get; }
        DependencyUrlType Type { get; }
    }
}