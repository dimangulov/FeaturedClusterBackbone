using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using TestProject1.Abstractions;
using TestProject1.WebServices;

namespace TestProject1.Cads;

public class CadHandlerProxy<TCadRequest, TResponse>: ICadHandler<TCadRequest, TResponse>
{
    private readonly IDistributedCache _distributedCache;
    private readonly IManagingNode _managingNode;
    private readonly IServiceScope _scope;
    private readonly IRemoteCadHandlerFactory _remoteCadHandlerFactory;
    private readonly ICluster _cluster;

    public CadHandlerProxy(
        IDistributedCache distributedCache, 
        IManagingNode managingNode, 
        IServiceScope scope,
        IRemoteCadHandlerFactory remoteCadHandlerFactory,
        ICluster cluster)
    {
        _distributedCache = distributedCache;
        _managingNode = managingNode;
        _scope = scope;
        _remoteCadHandlerFactory = remoteCadHandlerFactory;
        _cluster = cluster;
    }

    private async Task<IManagingNodeDescriptor> FindFirstAvailableNode()
    {
        const double memoryUsageThreshold = 0.9;

        if ((await _managingNode.Descriptor.GetMemoryUsageAsync()) < memoryUsageThreshold)
        {
            return _managingNode.Descriptor;
        }

        foreach (var nodeDescriptor in await _cluster.GetNodes())
        {
            if ((await nodeDescriptor.GetMemoryUsageAsync()) < memoryUsageThreshold)
            {
                return nodeDescriptor;
            }
        }

        //TODO wait and retry?
        throw new NotSupportedException("We don't have a node with enough memory for this operation, try again later");
    }

    public async Task<TResponse> HandleAsync(CadContextRequest<TCadRequest> request)
    {
        var cadKey = $"CadHandlerProxy-{request.OrganizationId}-{request.PrcId}";

        var uri = await _distributedCache.GetStringAsync(cadKey);

        if (uri == null)
        {
            var availableNode = await FindFirstAvailableNode();
            uri = availableNode.Url;
        }

        var handler = uri == _managingNode.Descriptor.Url
            ? _scope.ServiceProvider.GetRequiredKeyedService<ICadHandler<TCadRequest, TResponse>>("local")
            : await _remoteCadHandlerFactory.Get<TCadRequest, TResponse>(uri);
        
        return await handler.HandleAsync(request);
    }
}