namespace ClassLibrary1.Abstractions;

public interface ICluster
{
    Task Join(INodeHosting nodeHosting);
    Task Leave(INodeHosting nodeHosting);
    Task<INodeDescriptor[]> GetNodes();
}