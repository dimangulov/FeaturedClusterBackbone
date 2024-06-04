using ClassLibrary1.Abstractions;
using ClassLibrary1.WebServices;
using Microsoft.Extensions.Logging;
using Polly;

namespace ClassLibrary1.Cads;

public class CadHandlerProxy: ICadHandler
{
    private readonly ICadLocator _cadLocator;
    private readonly IRemoteCadHandlerFactory _remoteCadHandlerFactory;
    private readonly INodeAvailabilityStrategy _nodeAvailabilityStrategy;
    private readonly ILogger<CadHandlerProxy> _logger;

    public CadHandlerProxy(
        ICadLocator cadLocator,
        IRemoteCadHandlerFactory remoteCadHandlerFactory,
        INodeAvailabilityStrategy nodeAvailabilityStrategy,
        ILogger<CadHandlerProxy> logger)
    {
        _cadLocator = cadLocator;
        _remoteCadHandlerFactory = remoteCadHandlerFactory;
        _nodeAvailabilityStrategy = nodeAvailabilityStrategy;
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync<TCadRequest, TResponse>(TCadRequest request)
        where TCadRequest : ICadRequest
    {
        var handler = await FindOrInitializeHandler(request);

        return await handler.HandleAsync<TCadRequest, TResponse>(request);
    }

    private async Task<ICadHandler> FindOrInitializeHandler<TCadRequest>(
        TCadRequest request)
        where TCadRequest : ICadRequest
    {
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromSeconds(2 * attempt)
            );


        var uri = await _cadLocator.GetCadNode(request.OrganizationId, request.PrcId);

        var result = await policy.ExecuteAsync(async () =>
        {
            if (uri == null)
            {
                var node = await _nodeAvailabilityStrategy.GetAvailableNode();
                uri = node.Url;
            }

            var handler = await _remoteCadHandlerFactory.Get(uri);

            await handler.PingAsync();

            await _cadLocator.SetCadNode(request.OrganizationId, request.PrcId, uri);

            return handler;
        });

        return result;
    }

    public Task PingAsync()
    {
        throw new NotImplementedException();
    }
}