namespace TestProject1.Abstractions;

public interface IFeatureHandlerFactory
{
    Task<IFeatureHandler<TRequest, TResponse>> GetFeatureHandler<TRequest, TResponse>();
}