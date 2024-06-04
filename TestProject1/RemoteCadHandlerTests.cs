using ClassLibrary1.Cads;
using ClassLibrary1.WebServices;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace TestProject1;

public class RemoteCadHandlerTests
{
    private readonly Mock<ICadWebService> _service;
    private readonly RemoteCadHandler _handler;
    private Dictionary<Type, (string, Type)> _webserviceCallsToRequestMap;

    public RemoteCadHandlerTests()
    {
        _service = new Mock<ICadWebService>(MockBehavior.Strict);
        _handler = new RemoteCadHandler(_service.Object);

        FindSupportedWebServiceCalls();
    }

    private void FindSupportedWebServiceCalls()
    {
        _webserviceCallsToRequestMap = new Dictionary<Type, (string, Type)>();

        var requests = typeof(CadPresentedRequest).Assembly.GetTypes()
            .Where(t => t.GetInterface(nameof(ICadRequest)) != null)
            .ToArray();

        requests.Should().NotBeEmpty();

        var cadMethods = typeof(ICadWebService).GetMethods()
            .Where(x => x.GetCustomAttribute<IgnoreCadMethodAttribute>() == null)
            .ToArray();

        foreach (var requestType in requests)
        {
            var callMethod = 
                (from m in cadMethods
                let parameters = m.GetParameters()
                where parameters.Length == 1 && parameters[0].ParameterType == requestType
                select m).Single();

            var returnType = callMethod.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var taskResultType = returnType.GetGenericArguments()[0];
                returnType = taskResultType;
            }

            _webserviceCallsToRequestMap[requestType] = (callMethod.Name, returnType);
        }

        _webserviceCallsToRequestMap.Should().NotBeEmpty();
        _webserviceCallsToRequestMap.Should().HaveCount(cadMethods.Length);
    }

    [Fact]
    public async Task GenericAutoTests()
    {
        foreach (var requestReply in _webserviceCallsToRequestMap)
        {
            var requestObject = Activator.CreateInstance(requestReply.Key);
            var replyObject = Activator.CreateInstance(requestReply.Value.Item2);

            CallSetupWebService(requestObject, replyObject);

            await CallAutoTest(requestObject, replyObject);
        }
    }

    private async Task CallAutoTest(object? requestObject, object? replyObject)
    {
        var method = GetType().GetMethod(nameof(AutoTest), BindingFlags.NonPublic | BindingFlags.Instance);
        var genericMethod = method.MakeGenericMethod(requestObject.GetType(), replyObject.GetType());
        await (Task)genericMethod.Invoke(this, new[] { requestObject, replyObject });
    }

    private void CallSetupWebService(object? requestObject, object? replyObject)
    {
        var method = GetType().GetMethod(nameof(SetupWebService), BindingFlags.NonPublic | BindingFlags.Instance);
        var genericMethod = method.MakeGenericMethod(requestObject.GetType(), replyObject.GetType());
        genericMethod.Invoke(this, new[]{ requestObject, replyObject });
    }

    private async Task AutoTest<TRequest, TReply>(TRequest requestObject, TReply replyObject)
        where TRequest : ICadRequest
    {
        var result = await _handler.HandleAsync<TRequest, TReply>(requestObject);
        result.Should().Be(replyObject);
    }

    private void SetupWebService<TRequest, TReply>(TRequest requestObject, TReply replyObject)
    where TRequest: ICadRequest
    {
        var requestType = requestObject.GetType();
        if (!_webserviceCallsToRequestMap.ContainsKey(requestType))
        {
            throw new NotSupportedException($"{requestType.Name} doesn't have service call name configured");
        }

        var method = _webserviceCallsToRequestMap[requestType];

        var serviceParameter = Expression.Parameter(typeof(ICadWebService));
        var requestParam = Expression.Constant(requestObject);
        var expr = Expression.Call(serviceParameter, typeof(ICadWebService).GetMethod(method.Item1), requestParam);
        var lambda = Expression.Lambda<Func<ICadWebService, Task<TReply>>>(expr, serviceParameter);
        _service.Setup<Task<TReply>>(lambda).ReturnsAsync(replyObject);
    }
}