namespace TestProject1.Abstractions;

public interface IFeatureHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}