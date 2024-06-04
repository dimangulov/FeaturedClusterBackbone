using ClassLibrary1.WebServices;
using System.Reflection;

namespace ClassLibrary1.Cads;

public class RemoteCadHandler: ICadHandler
{
    private readonly ICadWebService _cadWebService;
    private static readonly Dictionary<Type, string> Configuration;

    static RemoteCadHandler()
    {
        Configuration = new Dictionary<Type, string>();

        var cadMethods = typeof(ICadWebService).GetMethods()
            .Where(x => x.GetCustomAttribute<IgnoreCadMethodAttribute>() == null)
            .ToArray();

        foreach (var cadMethod in cadMethods)
        {
            var returnType = cadMethod.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var taskResultType = returnType.GetGenericArguments()[0];
                returnType = taskResultType;
            }

            var parameter = cadMethod.GetParameters()[0];

            Configuration[parameter.ParameterType] = cadMethod.Name;
        }
    }

    public RemoteCadHandler(ICadWebService cadWebService)
    {
        _cadWebService = cadWebService;
    }

    public async Task<TResponse> HandleAsync<TCadRequest, TResponse>(TCadRequest request)
        where TCadRequest : ICadRequest
    {
        var serviceCallName = Configuration[typeof(TCadRequest)];

        var method = _cadWebService.GetType().GetMethod(serviceCallName);
        var result = method.Invoke(_cadWebService, new object?[] { request });
        var typedResult = await (Task<TResponse>) result!;
        return typedResult;
    }

    public async Task PingAsync()
    {
        _ = await _cadWebService.HealthyAsync();
    }
}