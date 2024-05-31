namespace TestProject1.WebServices;

public interface ICadWebServiceFactory
{
    Task<ICadWebService> Create(string url);
}