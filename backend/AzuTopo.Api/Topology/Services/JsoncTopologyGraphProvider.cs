using System.Text.Json;
using AzuTopo.Api.Topology.Configuration;
using AzuTopo.Api.Topology.Exceptions;
using AzuTopo.Api.Topology.Models;

namespace AzuTopo.Api.Topology.Services;

public sealed class JsoncTopologyGraphProvider : ITopologyGraphProvider
{
    private readonly TopologyProviderOptions options;
    private readonly TopologyEnvironmentConfig environmentConfig;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public JsoncTopologyGraphProvider(
        TopologyProviderOptions options,
        TopologyEnvironmentConfig environmentConfig)
    {
        this.options = options;
        this.environmentConfig = environmentConfig;
    }

    public async Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken)
    {
        var topologyGraph = await LoadTopologyAsync(cancellationToken);
        return topologyGraph;
    }

    private async Task<TopologyGraph> LoadTopologyAsync(CancellationToken cancellationToken)
    {
        ValidateProviderOptions();

        var fileInfo = new FileInfo(options.ConnectionsPath);
        if (!fileInfo.Exists)
        {
            throw new TopologyDefinitionException("Topology connections file was not found.");
        }

        if (fileInfo.Length > options.MaxFileSizeBytes)
        {
            throw new TopologyDefinitionException("Topology connections file is too large.");
        }

        try
        {
            await using var stream = fileInfo.OpenRead();
            var definition = await JsonSerializer.DeserializeAsync<TopologyDefinition>(
                stream,
                JsonOptions,
                cancellationToken);

            if (definition is null)
            {
                throw new TopologyDefinitionException("Topology connections file is empty.");
            }

            return BuildGraph(definition);
        }
        catch (JsonException ex)
        {
            throw new TopologyDefinitionException("Topology connections file is invalid.", ex);
        }
        catch (IOException ex)
        {
            throw new TopologyDefinitionException("Topology connections file could not be read.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new TopologyDefinitionException("Topology connections file could not be read.", ex);
        }
    }

    private void ValidateProviderOptions()
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionsPath))
        {
            throw new TopologyDefinitionException("Topology connections path is not configured.");
        }

        if (options.MaxFileSizeBytes <= 0)
        {
            throw new TopologyDefinitionException("Topology max file size must be positive.");
        }

        if (!Guid.TryParse(environmentConfig.SubscriptionId, out _))
        {
            throw new TopologyDefinitionException("Topology subscription id must be a GUID.");
        }
    }

    private TopologyGraph BuildGraph(TopologyDefinition definition)
    {
        var resources = definition.Resources ?? [];
        var connections = definition.Connections ?? [];
        var resolver = new PlaceholderResolver(environmentConfig.Placeholders);
        var nodes = resources.Select(resource => ToNode(resource, resolver)).ToArray();

        ValidateUniqueNodeIds(nodes);

        var nodeIds = nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var edges = connections
            .Select((connection, index) => ToEdge(connection, index, nodeIds, resolver))
            .ToArray();
        ValidateUniqueEdgeIds(edges);

        return new TopologyGraph(nodes, edges, Metrics: [], Warnings: []);
    }

    private static TopologyNode ToNode(TopologyResourceDefinition resource, PlaceholderResolver resolver)
    {
        var id = resolver.Resolve(Required(resource.Id, "resource.id"), "resource.id");
        var type = resolver.Resolve(Required(resource.Type, $"resource {id}.type"), $"resource {id}.type");
        var name = resolver.Resolve(Required(resource.Name, $"resource {id}.name"), $"resource {id}.name");

        return type switch
        {
            TopologyNodeTypes.Function => new TopologyNode(
                id,
                type,
                name,
                new FunctionNodeProperties(
                    resolver.Resolve(Required(resource.FunctionAppName, $"resource {id}.functionAppName"), $"resource {id}.functionAppName"),
                    resource.Enabled ?? throw new TopologyDefinitionException($"resource {id}.enabled is required."))),
            TopologyNodeTypes.ServiceBusQueue => new TopologyNode(
                id,
                type,
                name,
                new ServiceBusQueueNodeProperties(
                    resolver.Resolve(Required(resource.Namespace, $"resource {id}.namespace"), $"resource {id}.namespace"),
                    ValidQueueStatus(
                        resolver.Resolve(Required(resource.Status, $"resource {id}.status"), $"resource {id}.status"),
                        id))),
            _ => throw new TopologyDefinitionException($"resource {id}.type \"{type}\" is not supported."),
        };
    }

    private static TopologyEdge ToEdge(
        TopologyConnectionDefinition connection,
        int index,
        IReadOnlySet<string> nodeIds,
        PlaceholderResolver resolver)
    {
        var source = resolver.Resolve(Required(connection.Source, $"connections[{index}].source"), $"connections[{index}].source");
        var target = resolver.Resolve(Required(connection.Target, $"connections[{index}].target"), $"connections[{index}].target");
        var type = resolver.Resolve(Required(connection.Type, $"connections[{index}].type"), $"connections[{index}].type");

        if (!TopologyEdgeTypes.All.Contains(type))
        {
            throw new TopologyDefinitionException($"connections[{index}].type \"{type}\" is not supported.");
        }

        if (!nodeIds.Contains(source))
        {
            throw new TopologyDefinitionException($"connections[{index}].source \"{source}\" does not exist.");
        }

        if (!nodeIds.Contains(target))
        {
            throw new TopologyDefinitionException($"connections[{index}].target \"{target}\" does not exist.");
        }

        return new TopologyEdge($"{source}-{type}-{target}", source, target, type);
    }

    private static void ValidateUniqueNodeIds(IReadOnlyList<TopologyNode> nodes)
    {
        var duplicatedId = nodes
            .GroupBy(node => node.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicatedId is not null)
        {
            throw new TopologyDefinitionException($"resource.id \"{duplicatedId}\" is duplicated.");
        }
    }

    private static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new TopologyDefinitionException($"{fieldName} is required.");
        }

        return value;
    }

    private static string ValidQueueStatus(string status, string resourceId)
    {
        if (!ServiceBusQueueStatuses.All.Contains(status))
        {
            throw new TopologyDefinitionException($"resource {resourceId}.status \"{status}\" is not supported.");
        }

        return status;
    }

    private static void ValidateUniqueEdgeIds(IReadOnlyList<TopologyEdge> edges)
    {
        var duplicatedId = edges
            .GroupBy(edge => edge.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicatedId is not null)
        {
            throw new TopologyDefinitionException($"edge.id \"{duplicatedId}\" is duplicated.");
        }
    }
}
