namespace ClassLibrary1.WebServices;

public class CadWebService: ICadWebService
{
    private readonly string _uri;

    public CadWebService(string uri)
    {
        _uri = uri;
    }

    public Task<CadPresentedReply> CadPresentedAsync(CadPresentedRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HealthyAsync()
    {
        throw new NotImplementedException();
    }
}