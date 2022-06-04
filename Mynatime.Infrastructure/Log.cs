namespace Mynatime.Infrastructure;

using Microsoft.Extensions.Logging;
using System;

/// <summary>
/// Logging for Mynatime.
/// </summary>
public static class Log
{
    private static ILoggerFactory loggerFactory = null!;

    public static void SetLogger(ILoggerFactory thing)
    {
        loggerFactory = thing;
    }

    public static ILogger GetLogger<T>()
    {
        return loggerFactory.CreateLogger<T>();
    }

    public static ILogger GetLogger(string name)
    {
        return loggerFactory.CreateLogger(name);
    }
}
