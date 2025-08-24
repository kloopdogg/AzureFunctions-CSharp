using Moq;
using System.Net;
using SampleFunctionApp.Functions;
using Microsoft.Extensions.Logging;
using SampleFunctionApp.Tests.Fakes;
using Microsoft.Azure.Functions.Worker;
using Azure.Core.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SampleFunctionApp.Tests.Mocks;
using System.Text;
using Azure.Messaging.ServiceBus;

namespace SampleFunctionApp.Tests;

[TestClass]
public sealed class SampleAsbFunctionsTests
{
    private Mock<ILogger<SampleServiceBusFunctions>> _mockLogger = null!;
    private Mock<FunctionContext> _mockFunctionContext = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<SampleServiceBusFunctions>>();
        _mockFunctionContext = new Mock<FunctionContext>();
    }

    private static ServiceBusReceivedMessage CreateMessage(string? body, string? messageId = null, string? contentType = null)
    {
        var binary = body is null ? default : BinaryData.FromString(body);
        return ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: binary,
            messageId: messageId,
            contentType: contentType
        );
    }

    private SampleServiceBusFunctions CreateFunctions()
    {
        SampleServiceBusFunctions functions = new(_mockLogger.Object);
        return functions;
    }

    private void VerifyLog(LogLevel logLevel, string logMessage)
    {
        _mockLogger.Verify(
            x => x.Log<It.IsAnyType>(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() != null &&
                    v.ToString()!.Contains(logMessage, StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ProcessQueueMessage_ValidBody_NullMetadata_Success()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"name": "Test Event", "id": 42}"""
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: (null)");
        VerifyLog(LogLevel.Debug, "Message Content-Type: (null)");
        VerifyLog(LogLevel.Information, "Processing event: Test Event (42)");
        VerifyLog(LogLevel.Information, "Service Bus queue trigger function processed message (null)");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_ValidBody_EmptyMetadata_Success()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"name": "Test Event", "id": 42}""",
            messageId: string.Empty,
            contentType: string.Empty
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: ");
        VerifyLog(LogLevel.Debug, "Message Content-Type: ");
        VerifyLog(LogLevel.Information, "Processing event: Test Event (42)");
        VerifyLog(LogLevel.Information, "Service Bus queue trigger function processed message ");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_ValidBody_Success()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"name": "Test Event", "id": 42}""",
            messageId: "mid123",
            contentType: "application/json"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: application/json");
        VerifyLog(LogLevel.Information, "Processing event: Test Event (42)");
        VerifyLog(LogLevel.Information, "Service Bus queue trigger function processed message mid123");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_EmptyBody_Fail()
    {
        // Arrange
        var message = CreateMessage(
            body: """{}""",
            messageId: "mid123",
            contentType: "application/json"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: application/json");
        VerifyLog(LogLevel.Error, "Invalid SampleInfo: {}");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_MissingNameInBody_Fail()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"id": 404}""",
            messageId: "mid123",
            contentType: "application/json"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: application/json");
        VerifyLog(LogLevel.Error, """Invalid SampleInfo: {"id": 404}""");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_MissingIdInBody_Fail()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"name": "Missing"}""",
            messageId: "mid123",
            contentType: "application/json"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: application/json");
        VerifyLog(LogLevel.Error, """Invalid SampleInfo: {"name": "Missing"}""");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_NonJsonBody_Fail()
    {
        // Arrange
        var message = CreateMessage(
            body: "Plain string",
            messageId: "mid123",
            contentType: "text/plain"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: text/plain");
        VerifyLog(LogLevel.Error, "Invalid message body: Plain string");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_PlainTextBody_Success()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"name": "Test Event", "id": 42}""",
            messageId: "mid123",
            contentType: "text/plain"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: text/plain");
        VerifyLog(LogLevel.Information, "Processing event: Test Event (42)");
        VerifyLog(LogLevel.Information, "Service Bus queue trigger function processed message mid123");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_InvalidPlainTextBody_Fail()
    {
        // Arrange
        var message = CreateMessage(
            body: "{invalid json",
            messageId: "mid123",
            contentType: "text/plain"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: text/plain");
        VerifyLog(LogLevel.Error, "Invalid message body: {invalid json");
    }

    [TestMethod]
    public async Task ProcessQueueMessage_CustomContentTypeJsonBody_Success()
    {
        // Arrange
        var message = CreateMessage(
            body: """{"name": "Test Event", "id": 42}""",
            messageId: "mid123",
            contentType: "application/x-custom"
        );

        // Act
        SampleServiceBusFunctions functions = CreateFunctions();
        await functions.ProcessQueueMessage(message, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Debug, "Message ID: mid123");
        VerifyLog(LogLevel.Debug, "Message Content-Type: application/x-custom");
        VerifyLog(LogLevel.Information, "Processing event: Test Event (42)");
        VerifyLog(LogLevel.Information, "Service Bus queue trigger function processed message mid123");
    }
}
