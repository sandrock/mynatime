
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
    public const string DateInputFormat = "dd/MM/yyyy";
    public const string HourMinuteTimeFormat = "hh\\:mm";
    public static CultureInfo NumberLang { get; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// The Manatime login-page app-version this client build was validated against — the login
    /// page is the canary the runtime check reads (Manatime serves different per-frontend
    /// versions elsewhere). Bump it when the client is re-verified against a newer Manatime
    /// release; a higher version seen at runtime surfaces a drift notice. Snapshot: 7.1.0 on
    /// 2026-09-04, during an active 3.x -> 7.x rollout.
    /// </summary>
    public const string ExpectedManatimeVersion = "7.1.0";
}
