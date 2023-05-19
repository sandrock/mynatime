
namespace Mynatime.Client;

using System;
using System.Globalization;

/// <summary>
/// Constants related to the service. 
/// </summary>
#if CLIENTLIB
public static class ClientConstants
#else
internal static class ClientConstants
#endif
{
    public const string DateInputFormat = "yyyy-MM-dd";
    public const string HourMinuteTimeFormat = "hh\\:mm";
    public static CultureInfo NumberLang { get; } = CultureInfo.InvariantCulture;
}
