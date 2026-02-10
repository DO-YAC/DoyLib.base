using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace doylib.Logging;

public static class LoggerProvider
{
    private static ILoggerFactory sLoggerFactory = NullLoggerFactory.Instance;

    public static void SetLoggerFactory(ILoggerFactory? loggerFactory)
    {
        sLoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    public static ILogger<T> CreateLogger<T>()
    {
        return sLoggerFactory.CreateLogger<T>();
    }

    public static ILogger CreateLogger(string categoryName)
    {
        return sLoggerFactory.CreateLogger(categoryName);
    }
}
