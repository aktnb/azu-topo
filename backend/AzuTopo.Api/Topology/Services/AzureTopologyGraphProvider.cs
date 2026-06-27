using System.Text.Json;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.ServiceBus;
using Azure.ResourceManager.ServiceBus.Models;
using AzuTopo.Api.Topology.Configuration;
using AzuTopo.Api.Topology.Exceptions;
using AzuTopo.Api.Topology.Models;

namespace AzuTopo.Api.Topology.Services;

public sealed class AzureTopologyGraphProvider : ITopologyGraphProvider
{
    private readonly ArmClient armClient;
    private readonly TopologyEnvironmentConfig environmentConfig;
    private readonly TopologyProviderOptions providerOptions;
    private readonly ILogger<AzureTopologyGraphProvider> logger;

    private static readonly JsonSerializerOptions JsoncOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public AzureTopologyGraphProvider(
        ArmClient armClient,
        TopologyEnvironmentConfig environmentConfig,
        TopologyProviderOptions providerOptions,
        ILogger<AzureTopologyGraphProvider> logger)
    {
        this.armClient = armClient;
        this.environmentConfig = environmentConfig;
        this.providerOptions = providerOptions;
        this.logger = logger;
    }

    public async Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken)
    {
        var subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(
            environmentConfig.SubscriptionId);
        var subscription = armClient.GetSubscriptionResource(subscriptionResourceId);

        var warnings = new List<TopologyWarning>();

        var (functionNodes, bindingsByFunction) = await GetFunctionNodesAsync(
            subscription, cancellationToken);
        var queueNodes = await GetServiceBusQueueNodesAsync(subscription, cancellationToken);

        // queue name (case-insensitive) → list of queue nodes (multiple = ambiguous namespace)
        var queuesByName = queueNodes
            .GroupBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var allNodes = new List<TopologyNode>(functionNodes.Count + queueNodes.Count);
        allNodes.AddRange(functionNodes);
        allNodes.AddRange(queueNodes);

        var edges = new List<TopologyEdge>();
        var edgeSet = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (functionNodeId, bindings) in bindingsByFunction)
        {
            foreach (var binding in bindings)
            {
                ProcessBinding(binding, functionNodeId, queuesByName, edges, edgeSet, warnings);
            }
        }

        ApplyJsoncConnections(allNodes, edges, edgeSet, warnings);

        return new TopologyGraph(allNodes, edges, Metrics: [], Warnings: warnings);
    }

    private async Task<(
        List<TopologyNode> Nodes,
        List<(string NodeId, List<BindingInfo> Bindings)> BindingsByFunction)>
        GetFunctionNodesAsync(SubscriptionResource subscription, CancellationToken cancellationToken)
    {
        var nodes = new List<TopologyNode>();
        var bindingsByFunction = new List<(string, List<BindingInfo>)>();

        await foreach (var site in subscription.GetWebSitesAsync(cancellationToken: cancellationToken))
        {
            var kind = site.Data.Kind ?? string.Empty;
            if (!kind.Contains("functionapp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var appName = site.Data.Name;

            await foreach (var func in site.GetSiteFunctions().GetAllAsync(cancellationToken: cancellationToken))
            {
                var funcData = func.Data;

                // ARM returns the function name as "appName/funcName"
                var funcName = funcData.Name ?? string.Empty;
                var slashIdx = funcName.LastIndexOf('/');
                if (slashIdx >= 0)
                {
                    funcName = funcName[(slashIdx + 1)..];
                }

                if (string.IsNullOrEmpty(funcName))
                {
                    continue;
                }

                var nodeId = $"fn-{appName}-{funcName}".ToLowerInvariant();
                var enabled = funcData.IsDisabled is not true;

                nodes.Add(new TopologyNode(
                    nodeId,
                    TopologyNodeTypes.Function,
                    funcName,
                    new FunctionNodeProperties(appName, enabled)));

                var bindings = ParseBindings(funcData.Config, nodeId);
                bindingsByFunction.Add((nodeId, bindings));
            }
        }

        return (nodes, bindingsByFunction);
    }

    private static async Task<List<TopologyNode>> GetServiceBusQueueNodesAsync(
        SubscriptionResource subscription, CancellationToken cancellationToken)
    {
        var nodes = new List<TopologyNode>();

        await foreach (var ns in subscription.GetServiceBusNamespacesAsync(cancellationToken: cancellationToken))
        {
            var namespaceName = ns.Data.Name;

            await foreach (var queue in ns.GetServiceBusQueues().GetAllAsync(cancellationToken: cancellationToken))
            {
                var queueData = queue.Data;
                var queueName = queueData.Name;
                var nodeId = $"queue-{namespaceName}-{queueName}".ToLowerInvariant();
                var status = MapQueueStatus(queueData.Status);

                nodes.Add(new TopologyNode(
                    nodeId,
                    TopologyNodeTypes.ServiceBusQueue,
                    queueName,
                    new ServiceBusQueueNodeProperties(namespaceName, status)));
            }
        }

        return nodes;
    }

    private static List<BindingInfo> ParseBindings(BinaryData? config, string nodeId)
    {
        if (config is null)
        {
            return [];
        }

        try
        {
            var jsonOptions = new JsonDocumentOptions { MaxDepth = 32 };
            using var doc = JsonDocument.Parse(config.ToMemory(), jsonOptions);
            var root = doc.RootElement;

            if (!root.TryGetProperty("bindings", out var bindingsElement)
                || bindingsElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var result = new List<BindingInfo>();
            foreach (var binding in bindingsElement.EnumerateArray())
            {
                var type = binding.TryGetProperty("type", out var typeProp)
                    ? typeProp.GetString()
                    : null;
                var direction = binding.TryGetProperty("direction", out var dirProp)
                    ? dirProp.GetString()
                    : null;
                // trigger uses "queueName"; output binding uses "queueOrTopicName"
                var queueName = binding.TryGetProperty("queueName", out var qProp)
                    ? qProp.GetString()
                    : binding.TryGetProperty("queueOrTopicName", out var qtProp)
                        ? qtProp.GetString()
                        : null;
                var hasSubscriptionName = binding.TryGetProperty("subscriptionName", out _);

                result.Add(new BindingInfo(type, direction, queueName, hasSubscriptionName));
            }

            return result;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static void ProcessBinding(
        BindingInfo binding,
        string functionNodeId,
        Dictionary<string, List<TopologyNode>> queuesByName,
        List<TopologyEdge> edges,
        HashSet<string> edgeSet,
        List<TopologyWarning> warnings)
    {
        var isTrigger = string.Equals(
            binding.Type, "serviceBusTrigger", StringComparison.OrdinalIgnoreCase)
            && string.Equals(binding.Direction, "in", StringComparison.OrdinalIgnoreCase);
        var isOutput = string.Equals(
            binding.Type, "serviceBus", StringComparison.OrdinalIgnoreCase)
            && string.Equals(binding.Direction, "out", StringComparison.OrdinalIgnoreCase);

        if (!isTrigger && !isOutput)
        {
            return;
        }

        // Topic binding (subscriptionName present) → unsupportedBinding
        if (binding.HasSubscriptionName)
        {
            warnings.Add(new TopologyWarning(
                TopologyWarningCodes.UnsupportedBinding,
                $"Function {functionNodeId} has a Topic binding (subscriptionName is present), which is not supported.",
                functionNodeId));
            return;
        }

        var queueName = binding.QueueName;

        // App setting reference (%VARIABLE_NAME%) → unsupportedBinding
        if (queueName is not null && IsAppSettingReference(queueName))
        {
            warnings.Add(new TopologyWarning(
                TopologyWarningCodes.UnsupportedBinding,
                $"Function {functionNodeId} has a dynamic queueName \"{queueName}\" (app setting reference), which is not supported.",
                functionNodeId));
            return;
        }

        if (string.IsNullOrEmpty(queueName))
        {
            return;
        }

        // Resolve queue by name (namespace-agnostic)
        if (!queuesByName.TryGetValue(queueName, out var matchingQueues)
            || matchingQueues.Count == 0)
        {
            // No match → skip without warning per design
            return;
        }

        if (matchingQueues.Count > 1)
        {
            // Ambiguous (same queue name in multiple namespaces) → connectionNotFound
            warnings.Add(new TopologyWarning(
                TopologyWarningCodes.ConnectionNotFound,
                $"Queue \"{queueName}\" exists in multiple namespaces; cannot resolve edge for function {functionNodeId}.",
                functionNodeId));
            return;
        }

        var queueNodeId = matchingQueues[0].Id;
        string source, target, edgeType;

        if (isTrigger)
        {
            // trigger: queue → function
            source = queueNodeId;
            target = functionNodeId;
            edgeType = TopologyEdgeTypes.Trigger;
        }
        else
        {
            // output: function → queue
            source = functionNodeId;
            target = queueNodeId;
            edgeType = TopologyEdgeTypes.Output;
        }

        var edgeId = $"{source}-{edgeType}-{target}";
        if (edgeSet.Add(edgeId))
        {
            edges.Add(new TopologyEdge(edgeId, source, target, edgeType));
        }
    }

    private void ApplyJsoncConnections(
        List<TopologyNode> allNodes,
        List<TopologyEdge> edges,
        HashSet<string> edgeSet,
        List<TopologyWarning> warnings)
    {
        var connectionsPath = providerOptions.ConnectionsPath;
        if (string.IsNullOrWhiteSpace(connectionsPath) || !File.Exists(connectionsPath))
        {
            return;
        }

        TopologyDefinition? definition;
        try
        {
            var content = File.ReadAllText(connectionsPath);
            definition = JsonSerializer.Deserialize<TopologyDefinition>(content, JsoncOptions);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Failed to read jsonc connections file at {Path}.", connectionsPath);
            return;
        }

        if (definition?.Connections is null || definition.Connections.Count == 0)
        {
            return;
        }

        var nodeIds = allNodes.Select(n => n.Id).ToHashSet(StringComparer.Ordinal);
        var resolver = new PlaceholderResolver(environmentConfig.Placeholders);

        foreach (var connection in definition.Connections)
        {
            try
            {
                if (string.IsNullOrEmpty(connection.Source)
                    || string.IsNullOrEmpty(connection.Target)
                    || string.IsNullOrEmpty(connection.Type))
                {
                    warnings.Add(new TopologyWarning(
                        TopologyWarningCodes.ConnectionNotFound,
                        "jsonc connection is missing required fields (source, target, or type).",
                        null));
                    continue;
                }
                var source = resolver.Resolve(connection.Source, "connection.source");
                var target = resolver.Resolve(connection.Target, "connection.target");
                var type = resolver.Resolve(connection.Type, "connection.type");

                if (!nodeIds.Contains(source))
                {
                    warnings.Add(new TopologyWarning(
                        TopologyWarningCodes.ConnectionNotFound,
                        $"jsonc connection source \"{source}\" does not exist.",
                        source));
                    continue;
                }

                if (!nodeIds.Contains(target))
                {
                    warnings.Add(new TopologyWarning(
                        TopologyWarningCodes.ConnectionNotFound,
                        $"jsonc connection target \"{target}\" does not exist.",
                        target));
                    continue;
                }

                if (!TopologyEdgeTypes.All.Contains(type))
                {
                    warnings.Add(new TopologyWarning(
                        TopologyWarningCodes.ConnectionNotFound,
                        $"jsonc connection type \"{type}\" is not supported.",
                        null));
                    continue;
                }

                var edgeId = $"{source}-{type}-{target}";
                if (edgeSet.Add(edgeId))
                {
                    edges.Add(new TopologyEdge(edgeId, source, target, type));
                }
            }
            catch (TopologyDefinitionException ex)
            {
                warnings.Add(new TopologyWarning(
                    TopologyWarningCodes.ConnectionNotFound,
                    $"jsonc connection could not be resolved: {ex.Message}",
                    null));
            }
        }
    }

    private static bool IsAppSettingReference(string value)
        => value.Length > 2 && value.StartsWith('%') && value.EndsWith('%');

    private static string MapQueueStatus(ServiceBusMessagingEntityStatus? status)
    {
        return status switch
        {
            ServiceBusMessagingEntityStatus.Active => ServiceBusQueueStatuses.Active,
            ServiceBusMessagingEntityStatus.Disabled => ServiceBusQueueStatuses.Disabled,
            ServiceBusMessagingEntityStatus.SendDisabled => ServiceBusQueueStatuses.SendDisabled,
            ServiceBusMessagingEntityStatus.ReceiveDisabled => ServiceBusQueueStatuses.ReceiveDisabled,
            _ => ServiceBusQueueStatuses.Active,
        };
    }

    private sealed record BindingInfo(
        string? Type,
        string? Direction,
        string? QueueName,
        bool HasSubscriptionName);
}
