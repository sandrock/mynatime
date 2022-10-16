
namespace Mynatime.Infrastructure;

using System.Text.RegularExpressions;

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

    public static string DateFormat = "yyyy-MM-dd";

    public static string TimeFormat = "HHmm";
}
