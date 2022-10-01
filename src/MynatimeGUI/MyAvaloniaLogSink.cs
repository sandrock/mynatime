namespace Mynatime.GUI;

using Avalonia.Logging;
using Avalonia.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Writes Avalonia logs into the dotnet logger. See https://github.com/AvaloniaUI/Avalonia/blob/9bc3ce6ad85f0bbf129b65bf0042308ae3fdb205/src/Avalonia.Base/Logging/TraceLogSink.cs 
/// </summary>
internal class MyAvaloniaLogSink : Avalonia.Logging.ILogSink
{
    private readonly ILoggerFactory loggerFactory;
    private readonly LogEventLevel minimumLevel;
    private readonly Dictionary<string, ILogger?> areas;

    public MyAvaloniaLogSink(ILoggerFactory loggerFactory, LogEventLevel minimumLevel)
    {
        this.loggerFactory = loggerFactory;
        this.minimumLevel = minimumLevel;
        this.areas = new Dictionary<string, ILogger?>();
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        if (this.minimumLevel >= level)
        {
            if (this.areas.ContainsKey(area))
            {
                return true;
            }
        }

        return false;
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        var logger = this.GetLogger(level, area);
        if (logger != null)
        {
            logger.Log(this.ConvertLevel(level), Format<object, object, object>(area, messageTemplate, source));
        }
    }

    public void Log<T0>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0)
    {
        var logger = this.GetLogger(level, area);
        if (logger != null)
        {
            logger.Log(this.ConvertLevel(level), Format<object, object, object>(area, messageTemplate, source, propertyValue0));
        }
    }

    public void Log<T0, T1>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        var logger = this.GetLogger(level, area);
        if (logger != null)
        {
            logger.Log(this.ConvertLevel(level), Format<object, object, object>(area, messageTemplate, source, propertyValue0, propertyValue1));
        }
    }

    public void Log<T0, T1, T2>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        var logger = this.GetLogger(level, area);
        if (logger != null)
        {
            logger.Log(this.ConvertLevel(level), Format<object, object, object>(area, messageTemplate, source, propertyValue0, propertyValue1, propertyValue2));
        }
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        var logger = this.GetLogger(level, area);
        if (logger != null)
        {
            logger.Log(this.ConvertLevel(level), Format<object, object, object>(area, messageTemplate, source, propertyValues));
        }
    }

    private ILogger? GetLogger(LogEventLevel level, string area)
    {
        if (level >= this.minimumLevel && this.areas.TryGetValue(area, out ILogger? logger))
        {
            return logger;
        }

        return null;
    }

    private LogLevel ConvertLevel(LogEventLevel level)
    {
        if (level == LogEventLevel.Debug)
        {
            return LogLevel.Debug;
        }
        else if (level == LogEventLevel.Error)
        {
            return LogLevel.Error;
        }
        else if (level == LogEventLevel.Fatal)
        {
            return LogLevel.Error;
        }
        else if (level == LogEventLevel.Information)
        {
            return LogLevel.Information;
        }
        else if (level == LogEventLevel.Verbose)
        {
            return LogLevel.Debug;
        }
        else if (level == LogEventLevel.Warning)
        {
            return LogLevel.Warning;
        }
        else
        {
            return LogLevel.Warning;
        }
    }
    

    private static string Format<T0, T1, T2>(
        string area,
        string template,
        object? source,
        T0? v0 = default,
        T1? v1 = default,
        T2? v2 = default)
    {
        var result = new StringBuilder(template.Length);
        var r = new CharacterReader(template.AsSpan());
        var i = 0;

        result.Append('[');
        result.Append(area);
        result.Append("] ");

        while (!r.End)
        {
            var c = r.Take();

            if (c != '{')
            {
                result.Append(c);
            }
            else
            {
                if (r.Peek != '{')
                {
                    result.Append('\'');
                    result.Append(i++ switch
                    {
                        0 => v0,
                        1 => v1,
                        2 => v2,
                        _ => null
                    });
                    result.Append('\'');
                    r.TakeUntil('}');
                    r.Take();
                }
                else
                {
                    result.Append('{');
                    r.Take();
                }
            }
        }

        if (source is object)
        {
            result.Append(" (");
            result.Append(source.GetType().Name);
            result.Append(" #");
            result.Append(source.GetHashCode());
            result.Append(')');
        }

        return result.ToString();
    }
}
