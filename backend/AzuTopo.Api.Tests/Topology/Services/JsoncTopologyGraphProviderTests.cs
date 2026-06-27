using AzuTopo.Api.Topology.Configuration;
using AzuTopo.Api.Topology.Exceptions;
using AzuTopo.Api.Topology.Models;
using AzuTopo.Api.Topology.Services;
using NUnit.Framework;

namespace AzuTopo.Api.Tests.Topology.Services;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class JsoncTopologyGraphProviderTests
{
  private const string SubscriptionId = "11111111-1111-1111-1111-111111111111";

  private readonly List<string> temporaryDirectories = [];

  [TearDown]
  public void TearDown()
  {
    foreach (var temporaryDirectory in temporaryDirectories)
    {
      if (Directory.Exists(temporaryDirectory))
      {
        Directory.Delete(temporaryDirectory, recursive: true);
      }
    }

    temporaryDirectories.Clear();
  }

  [Test]
  public async Task GetTopologyAsync_LoadsJsoncFileAndResolvesPlaceholders()
  {
    var connectionsPath = await WriteConnectionsFileAsync(
        """
            {
              // JSONC comments and trailing commas are allowed.
              "resources": [
                {
                  "id": "function-${EnvironmentName}",
                  "type": "function",
                  "name": "Processor",
                  "functionAppName": "func-${EnvironmentName}",
                  "enabled": true,
                },
                {
                  "id": "queue-${EnvironmentName}",
                  "type": "serviceBusQueue",
                  "name": "Orders",
                  "namespace": "sb-${EnvironmentName}",
                  "status": "Active",
                },
              ],
              "connections": [
                {
                  "source": "queue-${EnvironmentName}",
                  "target": "function-${EnvironmentName}",
                  "type": "trigger",
                },
              ],
            }
            """);
    var provider = CreateProvider(connectionsPath);

    var graph = await provider.GetTopologyAsync(CancellationToken.None);

    using (Assert.EnterMultipleScope())
    {
      Assert.That(graph.Nodes, Has.Count.EqualTo(2));
      Assert.That(graph.Edges, Has.Count.EqualTo(1));
      Assert.That(graph.Warnings, Is.Empty);
      Assert.That(graph.Metrics, Is.Empty);

      var functionNode = graph.Nodes.Single(node => node.Id == "function-dev");
      Assert.That(functionNode.Type, Is.EqualTo(TopologyNodeTypes.Function));
      Assert.That(functionNode.Property, Is.TypeOf<FunctionNodeProperties>());
      var functionProperties = (FunctionNodeProperties)functionNode.Property;
      Assert.That(functionProperties.FunctionAppName, Is.EqualTo("func-dev"));
      Assert.That(functionProperties.Enabled, Is.True);

      var queueNode = graph.Nodes.Single(node => node.Id == "queue-dev");
      Assert.That(queueNode.Type, Is.EqualTo(TopologyNodeTypes.ServiceBusQueue));
      Assert.That(queueNode.Property, Is.TypeOf<ServiceBusQueueNodeProperties>());
      var queueProperties = (ServiceBusQueueNodeProperties)queueNode.Property;
      Assert.That(queueProperties.Namespace, Is.EqualTo("sb-dev"));
      Assert.That(queueProperties.Status, Is.EqualTo(ServiceBusQueueStatuses.Active));

      var edge = graph.Edges.Single();
      Assert.That(edge, Is.EqualTo(new TopologyEdge(
          "queue-dev-trigger-function-dev",
          "queue-dev",
          "function-dev",
          TopologyEdgeTypes.Trigger)));
    }
  }

  [Test]
  public async Task GetTopologyAsync_ThrowsWhenConnectionTargetDoesNotExist()
  {
    var connectionsPath = await WriteConnectionsFileAsync(
        """
            {
              "resources": [
                {
                  "id": "queue",
                  "type": "serviceBusQueue",
                  "name": "Orders",
                  "namespace": "sb-dev",
                  "status": "Active"
                }
              ],
              "connections": [
                {
                  "source": "queue",
                  "target": "missing-function",
                  "type": "trigger"
                }
              ]
            }
            """);
    var provider = CreateProvider(connectionsPath);

    using (Assert.EnterMultipleScope())
    {
      var exception = Assert.ThrowsAsync<TopologyDefinitionException>(
          () => provider.GetTopologyAsync(CancellationToken.None));

      Assert.That(exception?.Message, Is.EqualTo("connections[0].target \"missing-function\" does not exist."));
    }
  }

  private static JsoncTopologyGraphProvider CreateProvider(string connectionsPath)
  {
    return new JsoncTopologyGraphProvider(
        new TopologyProviderOptions { ConnectionsPath = connectionsPath },
        new TopologyEnvironmentConfig(
            "dev",
            SubscriptionId,
            new Dictionary<string, string>
            {
              ["EnvironmentName"] = "dev",
            }));
  }

  private async Task<string> WriteConnectionsFileAsync(string content)
  {
    var testDirectory = Path.Combine(
        Path.GetTempPath(),
        "azu-topo-tests",
        Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(testDirectory);
    temporaryDirectories.Add(testDirectory);

    var connectionsPath = Path.Combine(testDirectory, "topology.connections.jsonc");
    await File.WriteAllTextAsync(connectionsPath, content);

    return connectionsPath;
  }
}
