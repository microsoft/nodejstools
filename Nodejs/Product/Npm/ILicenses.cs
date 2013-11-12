namespace Microsoft.NodejsTools.Npm{
    public interface ILicenses{
        int Count { get; }
        ILicense this[int index] { get; }
    }
}