namespace ClassLibrary1.WebServices;

public interface ICadWebService
{
    Task<CadPresentedReply> CadPresentedAsync(CadPresentedRequest request);

    [IgnoreCadMethod]
    Task<bool> HealthyAsync();
}