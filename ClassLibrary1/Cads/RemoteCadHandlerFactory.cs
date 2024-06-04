using ClassLibrary1.WebServices;

namespace ClassLibrary1.Cads;

public class RemoteCadHandlerFactory: IRemoteCadHandlerFactory
{
    public Task<ICadHandler> Get(string uri)
    {
        return Task.FromResult<ICadHandler>(new RemoteCadHandler(new CadWebService(uri)));
    }
}