
namespace Mynatime.Client;

using System;
using System.Globalization;

public static class ClientConstants
{
    public const string DateInputFormat = "yyyy-MM-dd";
    public const string HourMinuteTimeFormat = "hh\\:mm";
    public static CultureInfo NumberLang { get; } = CultureInfo.InvariantCulture;
}
