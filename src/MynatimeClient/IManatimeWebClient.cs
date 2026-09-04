
namespace Mynatime.Client;

using Newtonsoft.Json.Linq;

public interface IManatimeWebClient
{
    JArray GetCookies();

    void SetCookies(JArray array);

    Task<BaseResult> PrepareEmailPasswordAuthenticate();

    Task<LoginResult> EmailPasswordAuthenticate(string loginUsername, string loginPassword);

    Task<HomeResult> GetHomepage();

    Task<NewActivityItemPage> GetNewActivityItemPage();

    Task<NewActivityItemPage> PostNewActivityItemPage(NewActivityItemPage form);

    /// <summary>
    /// Best-effort read of the service's app-version, without authenticating. Returns the
    /// numeric version (build suffix stripped) or null on any failure.
    /// </summary>
    Task<string?> TryGetAppVersionAsync();
}
