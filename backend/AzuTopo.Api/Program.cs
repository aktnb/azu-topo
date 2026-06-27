using AzuTopo.Api.Topology.Configuration;
using AzuTopo.Api.Topology.Exceptions;
using AzuTopo.Api.Topology.Models;
using AzuTopo.Api.Topology.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods("GET")
                .AllowAnyHeader();
        }
    });
});

var topologyEnvironment = CreateTopologyEnvironmentConfig(builder.Configuration);
var defaultConnectionsPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "topology.connections.jsonc");

builder.Services.AddSingleton(topologyEnvironment);
builder.Services.AddSingleton(new TopologyProviderOptions
{
    ConnectionsPath = Path.GetFullPath(defaultConnectionsPath),
});
builder.Services.AddSingleton<ITopologyGraphProvider, JsoncTopologyGraphProvider>();

var app = builder.Build();

app.UseCors();

app.MapGet("/api/topology", async (
    ITopologyGraphProvider topologyGraphProvider,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var graph = await topologyGraphProvider.GetTopologyAsync(cancellationToken);
        return Results.Ok(graph);
    }
    catch (TopologyDefinitionException ex)
    {
        logger.LogError(ex, "Failed to build topology graph from configured definition.");
        return Results.Problem(
            title: "Failed to build topology graph.",
            detail: "The configured topology definition is invalid.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetTopology")
.Produces<TopologyGraph>()
.ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();

static TopologyEnvironmentConfig CreateTopologyEnvironmentConfig(IConfiguration configuration)
{
    var environment = configuration["Topology:Environment"];
    if (string.IsNullOrWhiteSpace(environment))
    {
        throw new InvalidOperationException("Topology:Environment is required.");
    }

    var environmentSection = configuration.GetSection("Topology:Environments").GetSection(environment);
    if (!environmentSection.Exists())
    {
        throw new InvalidOperationException($"Topology:Environments:{environment} is not configured.");
    }

    var subscriptionId = environmentSection["SubscriptionId"];
    if (string.IsNullOrWhiteSpace(subscriptionId))
    {
        throw new InvalidOperationException($"Topology:Environments:{environment}:SubscriptionId is required.");
    }

    var placeholders = environmentSection
        .GetChildren()
        .Where(child => child.Value is not null
            && !string.Equals(child.Key, "SubscriptionId", StringComparison.OrdinalIgnoreCase))
        .ToDictionary(child => child.Key, child => child.Value!, StringComparer.Ordinal);

    return new TopologyEnvironmentConfig(environment, subscriptionId, placeholders);
}
