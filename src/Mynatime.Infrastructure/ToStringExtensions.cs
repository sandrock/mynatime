
namespace Mynatime.Infrastructure;

using System;
using System.Globalization;

// source: https://gist.github.com/sandrock/6fe3298b8ac6d9d0d9872dd811a63908

/// <summary>
/// Extension methods for common types that provide ToInvariantString() capability.
/// </summary>
public static class ToStringExtensions
{
    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this float value)
    {
        // Format R (roundtrip) is discouraged
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#the-round-trip-r-format-specifier
        // For Double values, the "R" format specifier in some cases fails to successfully round-trip the original value. For both Double and Single values, it also offers relatively poor performance. Instead, we recommend that you use the "G17" format specifier for Double values and the "G9" format specifier to successfully round-trip Single values.
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this double value)
    {
        // Format R (roundtrip) is discouraged
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#the-round-trip-r-format-specifier
        // For Double values, the "R" format specifier in some cases fails to successfully round-trip the original value. For both Double and Single values, it also offers relatively poor performance. Instead, we recommend that you use the "G17" format specifier for Double values and the "G9" format specifier to successfully round-trip Single values.
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this int value)
    {
        // Format R (roundtrip) is not available
        return value.ToString("D", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this long value)
    {
        // Format R (roundtrip) is not available
        return value.ToString("D", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this decimal value)
    {
        // Format R (roundtrip) is not available
        return value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <example>"2020-01-10T14:07:04.2921606+01:00"</example>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this DateTime value)
    {
        // Format R uses the ugly RFC1123 format
        // Format O uses the greatest of all ISO 8601 standard
        // The "O" or "o" standard format specifier represents a custom date and time format string using a pattern that preserves time zone information and emits a result string that complies with ISO 8601. 
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-round-trip-o-o-format-specifier
        return value.ToString("o");
    }

    /// <summary>
    /// Returns an invariant, roundtrip-safe string representation of this value.
    /// </summary>
    /// <example>"16.00:04:15.3840000"</example>
    /// <param name="value"></param>
    /// <returns>an invariant, roundtrip-safe string representation</returns>
    public static string ToInvariantString(this TimeSpan value)
    {
        // Format R (roundtrip) is not available
        // The "c" format specifier, unlike the "g" and "G" format specifiers, is not culture-sensitive. 
        // It produces the string representation of a TimeSpan value that is invariant and that is common to all previous versions of the .NET Framework before the .NET Framework 4.
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings#the-constant-c-format-specifier
        return value.ToString("c", CultureInfo.InvariantCulture);
    }
}
