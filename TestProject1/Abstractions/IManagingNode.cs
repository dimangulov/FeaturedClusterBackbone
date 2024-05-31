namespace TestProject1.Abstractions
{
    public interface IManagingNode
    {
        IManagingNodeDescriptor Descriptor { get; }
        Task StartAsync();
        Task StopAsync();
    }
}
