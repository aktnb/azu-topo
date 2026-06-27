using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString")
                ?? throw new InvalidOperationException("ServiceBusConnectionString is not configured.");
            return new ServiceBusClient(connectionString);
        });
    })
    .Build();

await host.RunAsync();
