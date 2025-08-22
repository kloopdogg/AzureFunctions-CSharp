using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SampleFunctionApp.Utils;

namespace SampleFunctionApp.Functions;

public class SampleTimerFunctions(ILogger<SampleTimerFunctions> _logger, ITimeProvider _timeProvider)
{
    [Function("ScheduledWork")]
    public async Task ScheduledWork([TimerTrigger("0 */5 * * * *", RunOnStartup = true, UseMonitor = false)] TimerInfo timerInfo,
        FunctionContext context)
    {
        var utcTimestamp = _timeProvider.UtcNow.ToString("O");

        if (timerInfo.IsPastDue)
        {
            _logger.LogInformation("Timer function is past due!");
        }

        // Simulate some work being done
        await Task.Delay(250, context.CancellationToken);

        _logger.LogInformation("C# timer trigger function ran at {Timestamp}", utcTimestamp);
    }
}