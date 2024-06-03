namespace ClassLibrary1.Abstractions
{
    /// <summary>
    /// The node should re-register in the cluster from time to time?
    /// Cluster should validate its state?
    /// </summary>
    public interface INodeHosting
    {
        INodeDescriptor Descriptor { get; }
        Task StartAsync();
        Task StopAsync();
    }
}
