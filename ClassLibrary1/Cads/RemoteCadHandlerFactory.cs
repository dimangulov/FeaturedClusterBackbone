using ClassLibrary1.WebServices;

namespace ClassLibrary1.Cads;

public class RemoteCadHandlerFactory: IRemoteCadHandlerFactory
{
    public Task<ICadHandler<TCadRequest, TResponse>> Get<TCadRequest, TResponse>(string uri) where TCadRequest : ICadRequest
    {
        throw new NotImplementedException();
    }
}