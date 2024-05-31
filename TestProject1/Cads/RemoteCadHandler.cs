using TestProject1.WebServices;

namespace TestProject1.Cads;

public class RemoteCadHandler<TCadRequest, TResponse> : ICadHandler<TCadRequest, TResponse>
{
    private readonly ICadWebService _cadWebService;

    public RemoteCadHandler(ICadWebService cadWebService)
    {
        _cadWebService = cadWebService;
    }

    public Task<TResponse> HandleAsync(CadContextRequest<TCadRequest> request)
    {
        throw new NotImplementedException();
    }
}