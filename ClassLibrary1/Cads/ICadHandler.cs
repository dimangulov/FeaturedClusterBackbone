using ClassLibrary1.WebServices;

namespace ClassLibrary1.Cads;

public interface ICadHandler<TRequest, TResponse>
    where TRequest: ICadRequest
{
    Task<TResponse> HandleAsync(TRequest request);
    Task PingAsync();
}