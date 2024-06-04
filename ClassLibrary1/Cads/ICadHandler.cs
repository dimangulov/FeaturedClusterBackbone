using ClassLibrary1.WebServices;

namespace ClassLibrary1.Cads;

public interface ICadHandler
{
    Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest request)
        where TRequest : ICadRequest;
    Task PingAsync();
}