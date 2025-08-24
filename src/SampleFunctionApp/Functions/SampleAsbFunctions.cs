using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SampleFunctionApp.Models;

namespace SampleFunctionApp.Functions;

public class SampleServiceBusFunctions(ILogger<SampleServiceBusFunctions> _logger)
{
    /// <summary>
    /// Processes messages from the Service Bus queue using a Service Bus trigger.
    /// This function uses autocomplete of messages with PeekLock mode, which means
    /// messages are automatically completed (removed from the queue) upon successful
    /// function execution. If the function fails, the message will be returned to
    /// the queue for retry processing.
    /// </summary>
    /// <param name="message">Service Bus message received from the queue containing the message data, properties, and metadata</param>
    /// <param name="context">Function execution context providing access to cancellation tokens and other runtime information</param>
    [Function("ProcessQueueMessage")]
    public async Task ProcessQueueMessage([ServiceBusTrigger("sample-queue", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        _logger.LogDebug("Message ID: {id}", message.MessageId);
        _logger.LogDebug("Message Content-Type: {contentType}", message.ContentType);

        SampleInfo? sampleInfo;
        try
        {
            sampleInfo = message.Body.ToObjectFromJson<SampleInfo>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid message body: {body}", message.Body);
            return;
        }

        if (sampleInfo is null || string.IsNullOrWhiteSpace(sampleInfo.Name) || sampleInfo.Id == 0)
        {
            _logger.LogError("Invalid SampleInfo: {body}", message.Body);
            return;
        }

        _logger.LogInformation("Processing event: {Name} ({ID})", sampleInfo.Name, sampleInfo.Id);

        // Simulate some work being done
        await Task.Delay(250, context.CancellationToken);

        _logger.LogInformation("Service Bus queue trigger function processed message {MessageId}", message.MessageId);
    }
}
