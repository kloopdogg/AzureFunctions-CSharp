namespace SampleFunctionApp.Utils;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
}

public class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

// public class TimeProvider
// {
//     private Func<DateTime> _provider;

//     /// <summary>
//     /// Initialize with default time provider.
//     /// </summary>
//     public TimeProvider()
//     {
//         _provider = GetCurrentTime;
//     }

//     /// <summary>
//     /// Get the current UTC time.
//     /// </summary>
//     private static DateTime GetCurrentTime()
//     {
//         return DateTime.UtcNow;
//     }

//     /// <summary>
//     /// Get the current time provider function.
//     /// </summary>
//     public Func<DateTime> Get()
//     {
//         return _provider;
//     }

//     /// <summary>
//     /// Set the time provider function.
//     /// </summary>
//     /// <param name="provider">A function that returns a DateTime object.</param>
//     public void Set(Func<DateTime> provider)
//     {
//         _provider = provider;
//     }

//     /// <summary>
//     /// Singleton instance
//     /// </summary>
//     public static TimeProvider Instance { get; } = new();
// }
