namespace AzuTopo.Api.Topology.Configuration;

public sealed record TopologyEnvironmentConfig(
    string Environment,
    string SubscriptionId,
    IReadOnlyDictionary<string, string> Placeholders);
