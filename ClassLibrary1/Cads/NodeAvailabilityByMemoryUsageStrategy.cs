using ClassLibrary1.Abstractions;
using Microsoft.Extensions.Logging;

namespace ClassLibrary1.Cads;

public class NodeAvailabilityByMemoryUsageStrategy: INodeAvailabilityStrategy
{
    private readonly ICluster _cluster;
    private readonly ILogger<NodeAvailabilityByMemoryUsageStrategy> _logger;

    public NodeAvailabilityByMemoryUsageStrategy(ICluster cluster, ILogger<NodeAvailabilityByMemoryUsageStrategy> logger)
    {
        _cluster = cluster;
        _logger = logger;
    }

    public async Task<INodeDescriptor> GetAvailableNode()
    {
        const double memoryUsageThreshold = 0.9;

        foreach (var nodeDescriptor in await _cluster.GetNodes())
        {
            try
            {
                if ((await nodeDescriptor.GetMemoryUsageAsync()) < memoryUsageThreshold)
                {
                    return nodeDescriptor;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

        }

        //TODO wait and retry?
        throw new NotSupportedException("We don't have a node with enough memory for this operation, try again later");
    }
}