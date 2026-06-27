namespace AzuTopo.Api.Topology.Exceptions;

public sealed class TopologyDefinitionException : Exception
{
    public TopologyDefinitionException(string message)
        : base(message)
    {
    }

    public TopologyDefinitionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
