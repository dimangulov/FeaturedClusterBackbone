namespace ClassLibrary1.WebServices;

public interface ICadWebServiceFactory
{
    Task<ICadWebService> Create(string url);
}