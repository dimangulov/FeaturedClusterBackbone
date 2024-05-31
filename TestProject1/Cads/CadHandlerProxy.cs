using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using TestProject1.Abstractions;
using TestProject1.WebServices;

namespace TestProject1.Cads;

public class CadHandlerProxy<TCadRequest, TResponse>: ICadHandler<TCadRequest, TResponse>
{
    private readonly IDistributedCache _distributedCache;
    private readonly IRemoteCadHandlerFactory _remoteCadHandlerFactory;
    private readonly ICluster _cluster;
    private readonly ILogger<CadHandlerProxy<TCadRequest, TResponse>> _logger;

    public CadHandlerProxy(
        IDistributedCache distributedCache, 
        IRemoteCadHandlerFactory remoteCadHandlerFactory,
        ICluster cluster,
        ILogger<CadHandlerProxy<TCadRequest, TResponse>> logger)
    {
        _distributedCache = distributedCache;
        _remoteCadHandlerFactory = remoteCadHandlerFactory;
        _cluster = cluster;
        _logger = logger;
    }

    private async Task<IManagingNodeDescriptor> FindAvailableNode()
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

    public async Task<TResponse> HandleAsync(CadContextRequest<TCadRequest> request)
    {
        var cadKey = $"CadHandlerProxy-{request.OrganizationId}-{request.PrcId}";

        var handler = await FindOrInitializeHandler(request, cadKey);

        return await handler.HandleAsync(request);
    }

    private async Task<ICadHandler<TCadRequest, TResponse>> FindOrInitializeHandler(
        CadContextRequest<TCadRequest> request, string cadKey)
    {
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromSeconds(10 * attempt)
            );


        var uri = await _distributedCache.GetStringAsync(cadKey);

        var result = await policy.ExecuteAsync(async () =>
        {
            if (uri == null)
            {
                var node = await FindAvailableNode();
                uri = node.Url;
            }

            var handler = await _remoteCadHandlerFactory.Get<TCadRequest, TResponse>(uri);

            await handler.PingAsync();

            await _distributedCache.SetStringAsync(cadKey, uri);

            return handler;
        });

        return result;
    }

    public Task PingAsync()
    {
        throw new NotImplementedException();
    }
}