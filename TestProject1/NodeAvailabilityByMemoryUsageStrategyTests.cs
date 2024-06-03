using ClassLibrary1.Abstractions;
using ClassLibrary1.Cads;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace TestProject1;

public class NodeAvailabilityByMemoryUsageStrategyTests
{
    private readonly Mock<ICluster> _cluster;
    private readonly NodeAvailabilityByMemoryUsageStrategy _strategy;
    private readonly List<INodeDescriptor> _nodes;
    private readonly Mock<ILogger<NodeAvailabilityByMemoryUsageStrategy>> _logger;

    public NodeAvailabilityByMemoryUsageStrategyTests()
    {
        _nodes = new List<INodeDescriptor>();

        _cluster = new Mock<ICluster>(MockBehavior.Strict);
        _cluster.Setup(c => c.GetNodes()).ReturnsAsync(() => _nodes.ToArray());
        _logger = new Mock<ILogger<NodeAvailabilityByMemoryUsageStrategy>>();
        _strategy = new NodeAvailabilityByMemoryUsageStrategy(_cluster.Object, _logger.Object);
    }

    [Fact]
    public async Task GetAvailableNode_ShouldThrowByDefault()
    {
        Func<Task> f = () => _strategy.GetAvailableNode();
        await f.Should().ThrowAsync<NotSupportedException>();
    }

    [Theory]
    [InlineData(0.85, true)]
    [InlineData(0.91, false)]
    public async Task SingleNode_GetAvailableNode_Theory(double memoryUsageThreshold, bool theNodeIsUsed)
    {
        Func<Task<INodeDescriptor>> f = () => _strategy.GetAvailableNode();

        var node = AddNode(memoryUsageThreshold);

        if (!theNodeIsUsed)
        {
            await f.Should().ThrowAsync<NotSupportedException>();
        }
        else
        {
            var result = await f();
            result.Should().Be(node);
        }
    }

    [Theory]
    [InlineData(0.85, true)]
    [InlineData(0.91, false)]
    public async Task MultiNode_GetAvailableNode_Theory(double memoryUsageThreshold, bool theNodeIsUsed)
    {
        //Several fully used nodes
        AddNode(0.9);
        AddNode(0.9);
        AddNode(0.9);

        Func<Task<INodeDescriptor>> f = () => _strategy.GetAvailableNode();

        var node = AddNode(memoryUsageThreshold);

        if (!theNodeIsUsed)
        {
            await f.Should().ThrowAsync<NotSupportedException>();
        }
        else
        {
            var result = await f();
            result.Should().Be(node);
        }
    }

    [Fact]
    public async Task GetAvailableNode_GetMemoryCanThrow()
    {
        Func<Task<INodeDescriptor>> f = () => _strategy.GetAvailableNode();

        var node = AddNode(0.5, true);

        var ex = await f.Should().ThrowAsync<Exception>();
        ex.WithMessage("We don't have a node with enough memory for this operation, try again later");
    }

    private INodeDescriptor AddNode(double memoryUsageThreshold, bool throws = false)
    {
        var n = new Mock<INodeDescriptor>(MockBehavior.Strict);
        n.Setup(x => x.GetMemoryUsageAsync()).ReturnsAsync(() =>
        {
            if (throws)
            {
                throw new Exception("GetMemoryUsageAsync");
            }
            return memoryUsageThreshold;
        });
        _nodes.Add(n.Object);
        return n.Object;
    }
}