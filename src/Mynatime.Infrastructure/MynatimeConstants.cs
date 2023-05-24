
namespace Mynatime.Infrastructure;

using System.Text.RegularExpressions;

/// <summary>
/// Constants related to the app. 
/// </summary>
public static class MynatimeConstants
{
    /// <summary>
    /// Parses an hour duration in decimal format.
    /// </summary>
    public static Regex DurationRegex = new Regex("^(\\d+)([\\.,](\\d+))?$");

    /// <summary>
    /// Parses a time in format "hhmm". 
    /// </summary>
    public static Regex TimeRegex = new Regex("^(\\d?\\d)(\\d\\d)$");

    public const string DateFormat = "yyyy-MM-dd";

    public const string TimeFormat = "HHmm";
    
    public const string ServiceBaseUrl = "https://app.manatime.com/";
}
