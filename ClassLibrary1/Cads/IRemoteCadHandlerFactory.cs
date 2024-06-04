namespace ClassLibrary1.Cads;

public interface IRemoteCadHandlerFactory
{
    Task<ICadHandler> Get(string uri);
}