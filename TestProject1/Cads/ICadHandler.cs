using TestProject1.Abstractions;

namespace TestProject1.Cads;

public interface ICadHandler<TCadRequest, TResponse>: IFeatureHandler<CadContextRequest<TCadRequest>, TResponse>
{
    Task PingAsync();
}