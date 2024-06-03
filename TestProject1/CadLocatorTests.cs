using System.Text;
using ClassLibrary1.Cads;
using ClassLibrary1.WebServices;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace TestProject1;

public class CadLocatorTests
{
    private readonly Mock<IDistributedCache> _cache;
    private readonly ICadLocator _cadLocator;

    public CadLocatorTests()
    {
        _cache = new Mock<IDistributedCache>(MockBehavior.Strict);
        _cadLocator = new CadLocator(_cache.Object);
    }

    [Fact]
    public async Task GetCadNode_ShouldReadCache()
    {
        var request = new CadPresentedRequest
        {
            OrganizationId = Guid.NewGuid().ToString(),
            PrcId = Guid.NewGuid().ToString(),
        };
        var cadKey = $"CadHandlerProxy-{request.OrganizationId}-{request.PrcId}";
        var expected = Guid.NewGuid().ToString();

        _cache.Setup(x => x.GetAsync(cadKey, default(CancellationToken))).ReturnsAsync((byte[]?)Encoding.UTF8.GetBytes(expected));

        var result = await _cadLocator.GetCadNode(request.OrganizationId, request.PrcId);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task SetCadNode_ShouldReadCache()
    {
        var request = new CadPresentedRequest
        {
            OrganizationId = Guid.NewGuid().ToString(),
            PrcId = Guid.NewGuid().ToString(),
        };
        var cadKey = $"CadHandlerProxy-{request.OrganizationId}-{request.PrcId}";
        var value = Guid.NewGuid().ToString();

        _cache.Setup(x => x.SetAsync(cadKey, Encoding.UTF8.GetBytes(value), It.IsAny<DistributedCacheEntryOptions>(), default(CancellationToken))).Returns(Task.CompletedTask);

        await _cadLocator.SetCadNode(request.OrganizationId, request.PrcId, value);
        _cache.VerifyAll();
    }

    [Fact]
    public async Task DeleteCadNode_ShouldReadCache()
    {
        var request = new CadPresentedRequest
        {
            OrganizationId = Guid.NewGuid().ToString(),
            PrcId = Guid.NewGuid().ToString(),
        };
        var cadKey = $"CadHandlerProxy-{request.OrganizationId}-{request.PrcId}";
        var value = Guid.NewGuid().ToString();

        _cache.Setup(x => x.RemoveAsync(cadKey, default(CancellationToken))).Returns(Task.CompletedTask);

        await _cadLocator.DeleteCadInformation(request.OrganizationId, request.PrcId);
        _cache.VerifyAll();
    }
}