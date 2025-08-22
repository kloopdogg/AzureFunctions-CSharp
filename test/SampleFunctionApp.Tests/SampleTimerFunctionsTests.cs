using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using SampleFunctionApp.Functions;
using SampleFunctionApp.Tests.Fakes;

namespace SampleFunctionApp.Tests;

[TestClass]
public sealed class SampleTimerFunctionsTests
{
    private Mock<ILogger<SampleTimerFunctions>> _mockLogger = null!;
    private Mock<FunctionContext> _mockFunctionContext = null!;
    private FakeTimeProvider _fakeTimeProvider = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<SampleTimerFunctions>>();
        _mockFunctionContext = new Mock<FunctionContext>();
        _fakeTimeProvider = new FakeTimeProvider();
    }

    private SampleTimerFunctions CreateFunctions()
    {
        SampleTimerFunctions functions = new(_mockLogger.Object, _fakeTimeProvider);
        return functions;
    }

    private void VerifyLog(LogLevel logLevel, string logMessage)
    {
        _mockLogger.Verify(
            x => x.Log<It.IsAnyType>(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(logMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ScheduledWork_NotPastDue_LogsTimestamp()
    {
        // Arrange
        TimerInfo timerInfo = new() { IsPastDue = false };
        string expectedTimestamp = _fakeTimeProvider.UtcNow.ToString("O");

        // Act
        SampleTimerFunctions functions = CreateFunctions();
        await functions.ScheduledWork(timerInfo, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Information, expectedTimestamp);
    }

    [TestMethod]
    public async Task ScheduledWork_PastDue_LogsPastDueMessage()
    {
        // Arrange
        TimerInfo timerInfo = new() { IsPastDue = true };

        // Act
        SampleTimerFunctions functions = CreateFunctions();
        await functions.ScheduledWork(timerInfo, _mockFunctionContext.Object);

        // Assert
        VerifyLog(LogLevel.Information, "Timer function is past due!");
    }

    [TestMethod]
    public async Task ScheduledWork_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        TimerInfo timerInfo = new() { IsPastDue = false };
        var cancellationTokenSource = new CancellationTokenSource();
        _mockFunctionContext.SetupGet(x => x.CancellationToken).Returns(cancellationTokenSource.Token);
        
        // Act & Assert
        SampleTimerFunctions functions = CreateFunctions();
        cancellationTokenSource.Cancel();
        
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(
            async () => await functions.ScheduledWork(timerInfo, _mockFunctionContext.Object)
        );
    }
}
