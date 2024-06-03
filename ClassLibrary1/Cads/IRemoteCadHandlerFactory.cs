using ClassLibrary1.WebServices;

namespace ClassLibrary1.Cads;

public interface IRemoteCadHandlerFactory
{
    Task<ICadHandler<TCadRequest, TResponse>> Get<TCadRequest, TResponse>(string uri) 
        where TCadRequest : ICadRequest;
}