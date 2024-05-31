namespace TestProject1.Abstractions;

public interface ICluster
{
    Task Join(IManagingNode node);
    Task Leave(IManagingNode node);
    Task<IManagingNodeDescriptor[]> GetNodes();
}