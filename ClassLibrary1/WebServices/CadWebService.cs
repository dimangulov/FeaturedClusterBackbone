namespace ClassLibrary1.WebServices;

public class CadWebService: ICadWebService
{
    public Task<CadPresentedReply> CadPresentedAsync(CadPresentedRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HealthyAsync()
    {
        throw new NotImplementedException();
    }
}