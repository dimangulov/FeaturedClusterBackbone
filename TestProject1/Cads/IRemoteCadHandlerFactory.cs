namespace TestProject1.Cads;

public interface IRemoteCadHandlerFactory
{
    Task<ICadHandler<TCadRequest, TResponse>> Get<TCadRequest, TResponse>(string uri);
}