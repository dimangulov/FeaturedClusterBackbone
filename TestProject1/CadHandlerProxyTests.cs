using Castle.Core.Logging;
using ClassLibrary1.Abstractions;
using ClassLibrary1.Cads;
using ClassLibrary1.WebServices;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace TestProject1
{
    public class CadHandlerProxyTests
    {
        private Mock<IRemoteCadHandlerFactory> _remoteCadHandlerFactory;
        private readonly Mock<INodeAvailabilityStrategy> _nodeAvailabilityStrategy;
        private readonly Mock<INodeDescriptor> _node;
        private readonly Mock<ICadHandler> _nodeCadHandler;
        private readonly Mock<ICadLocator> _cadLocator;

        public CadHandlerProxyTests()
        {
            _cadLocator = new Mock<ICadLocator>(MockBehavior.Strict);

            _remoteCadHandlerFactory = new Mock<IRemoteCadHandlerFactory>(MockBehavior.Strict);
            _nodeAvailabilityStrategy = new Mock<INodeAvailabilityStrategy>(MockBehavior.Strict);

            _node = new Mock<INodeDescriptor>(MockBehavior.Strict);
            _node.SetupGet(x => x.Url).Returns($"https://{Guid.NewGuid().ToString()}/");

            _nodeCadHandler = new Mock<ICadHandler>(MockBehavior.Strict);
            _nodeCadHandler.Setup(x => x.PingAsync()).Returns(Task.CompletedTask);
            _remoteCadHandlerFactory.Setup(x => x.Get(_node.Object.Url))
                .ReturnsAsync(_nodeCadHandler.Object);

            _nodeAvailabilityStrategy.Setup(x => x.GetAvailableNode()).ReturnsAsync(_node.Object);
        }

        [Fact]
        public async Task CadNotLoaded_CadPresentedRequest_ShouldBeLoaded()
        {
            var request = CreateNotLoadedRequest();

            var expectedResult = new CadPresentedReply
            {
                Found = true
            };

            SetupNodeCadHandler_Handle_Call(request, expectedResult);

            var proxy = BuildProxy();

            var result = await proxy.HandleAsync<CadPresentedRequest, CadPresentedReply>(request);

            result.Found.Should().Be(expectedResult.Found);
        }

        private void SetupNodeCadHandler_Handle_Call(CadPresentedRequest request, CadPresentedReply reply)
        {
            _nodeCadHandler.Setup(x => x.HandleAsync<CadPresentedRequest, CadPresentedReply>(request)).ReturnsAsync(reply);
        }

        [Fact]
        public async Task CadNotLoaded_CadPresentedRequest_ShouldPingTheHandler()
        {
            var request = CreateNotLoadedRequest();

            var expectedResult = new CadPresentedReply
            {
                Found = true
            };
            SetupNodeCadHandler_Handle_Call(request, expectedResult);

            var proxy = BuildProxy();

            var result = await proxy.HandleAsync<CadPresentedRequest, CadPresentedReply>(request);

            _nodeCadHandler.Verify(x => x.PingAsync());
        }

        [Fact]
        public async Task CadNotLoaded_CadPresentedRequest_ShouldSetTheNodeUriForTheCad()
        {
            var request = CreateNotLoadedRequest();

            var expectedResult = new CadPresentedReply
            {
                Found = true
            };
            SetupNodeCadHandler_Handle_Call(request, expectedResult);

            var proxy = BuildProxy();

            var result = await proxy.HandleAsync<CadPresentedRequest, CadPresentedReply>(request);

            _cadLocator.Verify(x => x.SetCadNode(request.OrganizationId, request.PrcId, _node.Object.Url));
        }

        [Fact]
        public async Task CadNotLoaded_CadPresentedRequest_ShouldTryPingTheHandlerSeveralTimes()
        {
            var request = CreateNotLoadedRequest();

            var expectedResult = new CadPresentedReply
            {
                Found = true
            };
            SetupNodeCadHandler_Handle_Call(request, expectedResult);

            var proxy = BuildProxy();

            var attempt1Thrown = false;
            _nodeCadHandler.Setup(x => x.PingAsync())
                .Returns(() =>
                {
                    if (!attempt1Thrown)
                    {
                        attempt1Thrown = true;
                        throw new Exception("Some random error");
                    }

                    return Task.CompletedTask;
                });

            var result = await proxy.HandleAsync<CadPresentedRequest, CadPresentedReply>(request);

            _nodeCadHandler.Verify(x => x.PingAsync());
        }

        private CadPresentedRequest CreateNotLoadedRequest()
        {
            var request = new CadPresentedRequest
            {
                OrganizationId = Guid.NewGuid().ToString(),
                PrcId = Guid.NewGuid().ToString(),
            };
            
            _cadLocator.Setup(x => x.GetCadNode(request.OrganizationId, request.PrcId)).ReturnsAsync((string?)null);

            _cadLocator.Setup(x => x.SetCadNode(request.OrganizationId, request.PrcId, It.IsAny<string>())).Returns(Task.CompletedTask);

            return request;
        }

        private (CadPresentedRequest, string) CreateLoadedRequest()
        {
            var nodeUri = Guid.NewGuid().ToString();
            var request = new CadPresentedRequest
            {
                OrganizationId = Guid.NewGuid().ToString(),
                PrcId = Guid.NewGuid().ToString(),
            };

            _cadLocator.Setup(x => x.GetCadNode(request.OrganizationId, request.PrcId)).ReturnsAsync(nodeUri);

            _cadLocator.Setup(x => x.SetCadNode(request.OrganizationId, request.PrcId, nodeUri)).Returns(Task.CompletedTask);

            return (request, nodeUri);
        }

        private ICadHandler BuildProxy() 
        {
            var logger = new Mock<ILogger<CadHandlerProxy>>();
            return new CadHandlerProxy(_cadLocator.Object, _remoteCadHandlerFactory.Object,
                _nodeAvailabilityStrategy.Object, logger.Object);
        }

        [Fact]
        public async Task CadLoaded_CadPresentedRequest_CustomHandlerShouldBeCalled()
        {
            var (request, nodeUri) = CreateLoadedRequest();

            var expectedResult = new CadPresentedReply
            {
                Found = true
            };

            var handler = SetupCustomCadHandler_Handle_Call(nodeUri, request, expectedResult);

            var proxy = BuildProxy();

            var result = await proxy.HandleAsync<CadPresentedRequest, CadPresentedReply>(request);

            result.Found.Should().Be(expectedResult.Found);
        }

        [Fact]
        public async Task CadLoaded_CadPresentedRequest_CustomHandlerShouldBePinged()
        {
            var (request, nodeUri) = CreateLoadedRequest();

            var expectedResult = new CadPresentedReply
            {
                Found = true
            };

            var handler = SetupCustomCadHandler_Handle_Call(nodeUri, request, expectedResult);

            var proxy = BuildProxy();

            var result = await proxy.HandleAsync<CadPresentedRequest, CadPresentedReply>(request);

            handler.Verify(x => x.PingAsync());
        }

        private Mock<ICadHandler> SetupCustomCadHandler_Handle_Call(string nodeUri, CadPresentedRequest request, CadPresentedReply reply)
        {
            var handler = new Mock<ICadHandler>(MockBehavior.Strict);

            handler.Setup(x => x.HandleAsync<CadPresentedRequest, CadPresentedReply>(request)).ReturnsAsync(reply);
            handler.Setup(x => x.PingAsync()).Returns(Task.CompletedTask);

            _remoteCadHandlerFactory.Setup(x => x.Get(nodeUri))
                .ReturnsAsync(handler.Object);

            return handler;
        }

        [Fact]
        public async Task PingIsNotUsed()
        {
            var proxy = BuildProxy();
            Func<Task> f = () => proxy.PingAsync();
            await f.Should().ThrowAsync<NotImplementedException>();
        }
    }
}