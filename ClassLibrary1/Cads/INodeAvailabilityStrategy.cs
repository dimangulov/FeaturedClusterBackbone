using ClassLibrary1.Abstractions;

namespace ClassLibrary1.Cads;

public interface INodeAvailabilityStrategy
{
    Task<INodeDescriptor> GetAvailableNode();
}