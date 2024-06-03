namespace ClassLibrary1.WebServices;

public interface ICadWebService
{
    Task<CadPresentedReply> CadPresentedAsync(CadPresentedRequest request);
    Task<bool> HealthyAsync();
}