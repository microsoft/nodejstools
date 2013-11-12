namespace Microsoft.NodejsTools.Npm{
    public interface IScripts{
        int Count { get; }
        IScript this[string name] { get; }
    }
}