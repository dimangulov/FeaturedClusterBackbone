namespace TestProject1.Cads;

public class LocalCadHandler<TCadRequest, TResponse> : ICadHandler<TCadRequest, TResponse>
{
    public Task<TResponse> HandleAsync(CadContextRequest<TCadRequest> request)
    {
        throw new NotImplementedException();
    }
}