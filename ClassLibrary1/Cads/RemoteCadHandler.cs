using ClassLibrary1.WebServices;

namespace ClassLibrary1.Cads;

public class RemoteCadHandler<TCadRequest, TResponse> : ICadHandler<TCadRequest, TResponse> where TCadRequest : ICadRequest
{
    private readonly ICadWebService _cadWebService;

    public RemoteCadHandler(ICadWebService cadWebService)
    {
        _cadWebService = cadWebService;
    }

    public Task<TResponse> HandleAsync(TCadRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task PingAsync()
    {
        _ = await _cadWebService.HealthyAsync();
    }
}