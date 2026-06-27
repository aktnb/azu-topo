using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzuTopo.TestFunctionApp.Functions;

public class ProcessOrderOutput
{
    [ServiceBusOutput("orders", Connection = "ServiceBusConnectionString")]
    public string? QueueMessage { get; set; }
    public HttpResponseData? HttpResponse { get; set; }
}

public class ProcessOrderFunction(ILogger<ProcessOrderFunction> logger)
{
    [Function("ProcessOrder")]
    public async Task<ProcessOrderOutput> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync();
        logger.LogInformation("ProcessOrder triggered. Body length: {Length}", body?.Length ?? 0);

        var orderId = Guid.NewGuid().ToString();
        var message = JsonSerializer.Serialize(new { OrderId = orderId, ReceivedAt = DateTimeOffset.UtcNow, Payload = body });

        var response = req.CreateResponse(HttpStatusCode.Accepted);
        await response.WriteAsJsonAsync(new { OrderId = orderId });

        return new ProcessOrderOutput { QueueMessage = message, HttpResponse = response };
    }
}
