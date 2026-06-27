using AzuTopo.Api.Topology.Models;

namespace AzuTopo.Api.Topology.Services;

public interface ITopologyGraphProvider
{
    Task<TopologyGraph> GetTopologyAsync(CancellationToken cancellationToken);
}
