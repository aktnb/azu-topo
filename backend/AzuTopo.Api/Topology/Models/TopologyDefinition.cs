using System.Text.Json.Serialization;

namespace AzuTopo.Api.Topology.Models;

internal sealed record TopologyDefinition(
    [property: JsonPropertyName("resources")] IReadOnlyList<TopologyResourceDefinition>? Resources,
    [property: JsonPropertyName("connections")] IReadOnlyList<TopologyConnectionDefinition>? Connections);

internal sealed record TopologyResourceDefinition(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("functionAppName")] string? FunctionAppName,
    [property: JsonPropertyName("enabled")] bool? Enabled,
    [property: JsonPropertyName("namespace")] string? Namespace,
    [property: JsonPropertyName("status")] string? Status);

internal sealed record TopologyConnectionDefinition(
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("target")] string? Target,
    [property: JsonPropertyName("type")] string? Type);
