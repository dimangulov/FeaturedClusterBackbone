using ClassLibrary1.Cads;
using FluentAssertions;
using Xunit;

namespace TestProject1;

public class RemoteCadHandlerFactoryTests
{
    [Fact]
    public async Task Get_ShouldCreateANewInstance()
    {
        var factory = new RemoteCadHandlerFactory();
        var uri = Guid.NewGuid().ToString();
        var result = await factory.Get(uri);
        result.Should().BeOfType<RemoteCadHandler>();
    }
}