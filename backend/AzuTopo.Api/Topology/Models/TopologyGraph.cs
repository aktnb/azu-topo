namespace AzuTopo.Api.Topology.Models;

public sealed record TopologyGraph(
    IReadOnlyList<TopologyNode> Nodes,
    IReadOnlyList<TopologyEdge> Edges,
    IReadOnlyList<TopologyMetric>? Metrics,
    IReadOnlyList<TopologyWarning> Warnings);

public sealed record TopologyNode(
    string Id,
    string Type,
    string Name,
    object Property);

public sealed record FunctionNodeProperties(
    string FunctionAppName,
    bool Enabled);

public sealed record ServiceBusQueueNodeProperties(
    string Namespace,
    string Status);

public sealed record TopologyEdge(
    string Id,
    string Source,
    string Target,
    string Type);

public sealed record TopologyMetric(
    string NodeId,
    IReadOnlyList<MetricValue> Values);

public sealed record MetricValue(
    string Name,
    double? Value,
    string? Unit);

public sealed record TopologyWarning(
    string Code,
    string Message,
    string? NodeId);
