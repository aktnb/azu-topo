namespace AzuTopo.Api.Topology.Configuration;

public sealed class TopologyProviderOptions
{
    public required string ConnectionsPath { get; init; }

    public long MaxFileSizeBytes { get; init; } = 64 * 1024;
}
