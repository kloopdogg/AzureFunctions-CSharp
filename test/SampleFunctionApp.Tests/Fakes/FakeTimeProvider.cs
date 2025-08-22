using SampleFunctionApp.Utils;

namespace SampleFunctionApp.Tests.Fakes;

public class FakeTimeProvider : ITimeProvider
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
