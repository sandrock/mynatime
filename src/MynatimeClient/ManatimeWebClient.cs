namespace Mynatime.Client;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

/// <summary>
/// Stateful client for the service. It holds cookie and page state. 
/// </summary>
public class ManatimeWebClient : IManatimeWebClient
{
    private readonly ILogger log;
    private readonly string baseUrl = "https://app.manatime.com/";
    private readonly List<LogEntry> logs = new List<LogEntry>();
    private bool canEmailPasswordAuthenticate;
    private string? csrfToken;

    public ManatimeWebClient(ILogger<ManatimeWebClient> log)
    {
        this.log = log;
        this.HttpHandler = new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
        };
        this.Http = new HttpClient(this.HttpHandler);
    }

    public ManatimeWebClient()
    {
        this.log = Mynatime.Infrastructure.Log.GetLogger<ManatimeWebClient>();
        this.HttpHandler = new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
        };
        this.Http = new HttpClient(this.HttpHandler);
    }

    public HttpClientHandler HttpHandler { get; set; }

    public HttpClient Http { get; set; }

    public async Task<BaseResult> PrepareEmailPasswordAuthenticate()
    {
        try
        {
            var request = this.CreateRequest(HttpMethod.Get, "security/login");
            
            // Cookie: hl=fr; PHPSESSID=xxxxx
            var response = await this.Send(nameof(this.PrepareEmailPasswordAuthenticate), request);
            if (response.IsSuccessStatusCode)
            {
                var contents = await response.Content.ReadAsStringAsync();
                var match = Regex.Match(contents, @"<input\s+type=""hidden""\s+name=""_csrf_token""\s+value=""([^""]+)""");
                if (match.Success)
                {
                    this.SetCsrfToken(nameof(this.PrepareEmailPasswordAuthenticate), match.Groups[1].Value);
                    this.canEmailPasswordAuthenticate = true;
                    return this.Log(BaseResult.Success<BaseResult>());
                }
                else
                {
                    return this.Log(BaseResult.Error<BaseResult>(ErrorCode.PageParseMissCsrf, "Authentication prepare failed. "));
                }
            }
            else
            {
                return this.Log(BaseResult.Error<LoginResult>(ErrorCode.UnknownError, "Other error. "));
            }
        }
        catch (Exception ex)
        {
            this.Log(nameof(this.PrepareEmailPasswordAuthenticate), ex);
            return this.Log(BaseResult.Error<BaseResult>(ErrorCode.UnknownError, "Authentication prepare failed. "));
        }
    }

    public async Task<LoginResult> EmailPasswordAuthenticate(string username, string password)
    {
        if (!this.canEmailPasswordAuthenticate || this.csrfToken == null)
        {
            return BaseResult.Error<LoginResult>(ErrorCode.MissingCsrfToken, "Cannot authenticate without the CSRF token. ");
        }
        /*
<form action="/security/login" method="post">
    <input type="hidden"                    name="_csrf_token"   value="kCCyMAVBpsQjea3O4vgnyHOpERWK54mB3pJZH1kBGQQ"/>
    <input type="text" id="username"        name="_username"     value="" required="required" autofocus="autofocus"/>
    <input type="password" id="password"    name="_password"     required="required"/>
    <input type="checkbox" id="remember_me" name="_remember_me"  value="on"/>&nbsp;Se souvenir de moi
    <button type="submit" class="btn btn-primary ml-3" id="_submit" name="_submit">
</form>
         */
        var request = this.CreateRequest(HttpMethod.Post, "security/login");
        var requestData = new List<KeyValuePair<string, string>>();
        requestData.Add(new KeyValuePair<string, string>("_csrf_token", this.csrfToken));
        requestData.Add(new KeyValuePair<string, string>("_username", username));
        requestData.Add(new KeyValuePair<string, string>("_password", password));
        requestData.Add(new KeyValuePair<string, string>("_remember_me", "on"));
        requestData.Add(new KeyValuePair<string, string>("_submit", ""));
        var requestContent = new FormUrlEncodedContent(requestData);
        request.Content = requestContent;

        // NOTE: on success, we get a HTTP 302, redirecting to "/utilisateur/", redirecting to "/home" (200)
        // NOTE: remember_me will generate a long-lived cookie
        // set-cookie: PHPSESSID=xxxxx; path=/; HttpOnly
        // set-cookie: REMEMBERME=deleted; expires=Mon, 24-May-2021 16:13:12 GMT; Max-Age=0;       path=/; httponly
        // set-cookie: REMEMBERME=xxxxxxx; expires=Thu, 23-Jun-2022 16:53:17 GMT; Max-Age=2592000; path=/; httponly
        var response = await this.Send(nameof(this.EmailPasswordAuthenticate), request);
        if (response.IsSuccessStatusCode)
        {
            var result = new LoginResult();

            var contents = await response.Content.ReadAsStringAsync();

            if (this.CheckPage(ManatimePage.SecurityLogin, contents, null))
            {
                result.AddError(new BaseError(ErrorCode.InvalidUsernameOrPassword, "Invalid credentials. "));
                return this.Log(result);
            }

            this.ParsePageResult(contents, result);

            return this.Log(result);
        }
        else
        {
            return this.Log(BaseResult.Error<LoginResult>(ErrorCode.UnknownError, "Other error. "));
        }
    }

    public async Task<HomeResult> GetHomepage()
    {
        var request = this.CreateRequest(HttpMethod.Get, "home");
        var response = await this.Send(nameof(this.EmailPasswordAuthenticate), request);
        if (response.IsSuccessStatusCode)
        {
            var result = new HomeResult();

            var contents = await response.Content.ReadAsStringAsync();

            if (!this.CheckPage(ManatimePage.Home, contents, result))
            {
                return this.Log(result);
            }

            this.ParsePageResult(contents, result);

            return this.Log(result);
        }
        else
        {
            return this.Log(BaseResult.Error<HomeResult>(ErrorCode.UnknownError, "Other error. "));
        }
    }

    public async Task<NewActivityItemPage> GetNewActivityItemPage()
    {
        var request = this.CreateRequest(HttpMethod.Get, "presences/create/advanced");
        var response = await this.Send(nameof(GetNewActivityItemPage), request);
        if (response.IsSuccessStatusCode)
        {
            var result = new NewActivityItemPage();
            result.LoadTime = DateTime.UtcNow;
            var contents = await response.Content.ReadAsStringAsync();
            if (!this.CheckPage(ManatimePage.PresenceCreateAdvanced, contents, result))
            {
                return this.Log(result);
            }

            result.ReadPage(contents);
            return this.Log(result);
        }
        else
        {
            return this.Log(BaseResult.Error<NewActivityItemPage>(ErrorCode.UnknownError, "Other error. "));
        }
    }

    public async Task<NewActivityItemPage> PostNewActivityItemPage(NewActivityItemPage form)
    {
        if (form == null)
        {
            throw new ArgumentNullException(nameof(form));
        }

        var request = this.CreateRequest(HttpMethod.Post, "presences/create/advanced");
        var requestData = form.WebForm.GetPairs();
        var requestContent = new FormUrlEncodedContent(requestData);
        request.Content = requestContent;

        var response = await this.Send(nameof(PostNewActivityItemPage), request);
        if (response.IsSuccessStatusCode)
        {
            var result = new NewActivityItemPage();
            result.LoadTime = DateTime.UtcNow;
            var contents = await response.Content.ReadAsStringAsync();
            if (!this.CheckPage(ManatimePage.PresenceCreateAdvanced, contents, result))
            {
                return this.Log(result);
            }

            result.ReadPage(contents);
            return this.Log(result);
        }
        else
        {
            return this.Log(BaseResult.Error<NewActivityItemPage>(ErrorCode.UnknownError, "Other error. "));
        }
    }

    public async Task<ActivityListPage> GetActivityListPage()
    {
        var request = this.CreateRequest(HttpMethod.Get, "presences/search");
        var response = await this.Send(nameof(GetActivityListPage), request);
        var result = new ActivityListPage();
        if (response.IsSuccessStatusCode)
        {
            result.LoadTime = DateTime.UtcNow;
            var contents = await response.Content.ReadAsStringAsync();
            if (!this.CheckPage(ManatimePage.PresenceSearch, contents, result))
            {
                return this.Log(result);
            }

            try
            {
                result.ReadPage(contents);
            }
            catch (Exception ex)
            {
                result.AddError(new BaseError(ErrorCode.InvalidPage.Generic, "Loaded page is wrong. "));
            }
        }
        else
        {
            result.AddError(new BaseError(ErrorCode.UnknownError, "Other error. "));
        }

        return this.Log(result);
    }

    public JArray GetCookies()
    {
        var array = new JArray();
        foreach (Cookie cookie in this.HttpHandler.CookieContainer.GetAllCookies())
        {
            var obj = new JObject();
            obj["Domain"] = cookie.Domain;
            obj["Name"] = cookie.Name;
            obj["Path"] = cookie.Path;
            obj["Value"] = cookie.Value;
            obj["Expires"] = cookie.Expires > default(DateTime) ? cookie.Expires.ToString("o") : default(string);
            obj["HttpOnly"] = cookie.HttpOnly;
            array.Add(obj);
        }
        
        return array;
    }

    public void SetCookies(JArray array)
    {
        foreach (JObject item in array)
        {
            var cookie = new Cookie(
                item.Value<string>("Name"),
                item.Value<string>("Value"),
                item.Value<string>("Path"),
                item.Value<string>("Domain"));
            this.HttpHandler.CookieContainer.Add(cookie);
        }
    }

    private Uri GetUri(string path)
    {
        return new Uri(this.baseUrl + path, UriKind.Absolute);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var uri = this.GetUri(path);
        var request = new HttpRequestMessage(method, uri);
        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:100.0) Gecko/20100101 Firefox/100.0");
        return request;
    }

    private void SetCsrfToken(string methodName, string value)
    {
        this.csrfToken = value;
        this.Log(methodName, "New CSRF token: " + this.csrfToken);
    }

    private void Log(string methodName, HttpRequestMessage request)
    {
        var entry = new LogEntry(methodName);
        entry.SetRequest(request);
        this.logs.Add(entry);

        if (this.log.IsEnabled(LogLevel.Trace))
        {
            this.log.LogTrace(entry.ToString());
        }
        else
        {
            this.log.LogDebug("HTTP Sending  " + request.Method + " " + request.RequestUri);
        }
    }

    private void Log(string methodName, HttpResponseMessage response)
    {
        var entry = new LogEntry(methodName);
        entry.SetResponse(response);
        this.logs.Add(entry);
        if (this.log.IsEnabled(LogLevel.Trace))
        {
            this.log.LogTrace(entry.ToString());
        }
        else
        {
            this.log.LogDebug("HTTP Received " + response.StatusCode + " " + response.ReasonPhrase);
        }
    }

    private void Log(string methodName, string message)
    {
        var entry = new LogEntry(methodName);
        entry.SetMessage(message);
        this.logs.Add(entry);
        this.log.LogInformation(entry.ToString());
    }

    private T Log<T>(T result, [CallerMemberName] string methodName = null)
        where T : BaseResult
    {
        var entry = new LogEntry(methodName);
        entry.SetResult(result);
        this.logs.Add(entry);
        this.log.LogInformation(entry.ToString());
        return result;
    }

    private async Task<HttpResponseMessage> Send(string methodName, HttpRequestMessage request)
    {
        this.Log(methodName, request);
        var response = await this.Http.SendAsync(request);
        this.Log(methodName, response);
        return response;
    }

    private void Log(string methodName, Exception exception)
    {
        var entry = new LogEntry(methodName);
        entry.SetException(exception);
        this.logs.Add(entry);
    }

    private bool CheckPage(ManatimePage desiredPage, string contents, BaseResult result)
    {
        //
        // current page is mostly detected using the javascript user tracking code
        // amplitude.track("Page View", {page: "Page View", {page: "security_login"});
        //
        bool isOkay = false, isLoggedOut = false;
        if (IsAnalyticsPage(contents, "security_login"))
        {
            isLoggedOut = true;
            isOkay = desiredPage == ManatimePage.SecurityLogin;
        }

        if (desiredPage == ManatimePage.Home)
        {
            // on 2023-07-03: this does not exist any more :(
            // on 2023-11: they changed from "analytics" to "amplitude"
            //   amplitude.track("Page View", {page: "Page View", {page: "security_login"});
            isOkay = IsAnalyticsPage(contents, "legacy_home");

            if (!isOkay)
            {
                // <span class="company text-muted text-truncate">MyCompany</span>
                isOkay = Regex.IsMatch(contents, "<span +class=\"(.+ )?company( .*)?\">.+</span>");
            }
        }
        else if (desiredPage == ManatimePage.PresenceCreateAdvanced)
        {
            // on 2023-07-03: this does not exist any more
            isOkay = IsAnalyticsPage(contents, "presence_create_advanced");
            
            // 2023-07-03: <title>Créer une présence > Avancé</title>
            // <form name="create" method="post" action="/presences/create/advanced">
            isOkay = contents.Contains("<title>Créer une présence > Avancé</title>")
                  || contents.Contains("""<form name="create" method="post" action="/presences/create/advanced">""");
        }
        else if (desiredPage == ManatimePage.PresenceSearch)
        {
            // 2024-06-13: """amplitude.track("Presence", {"""
            isOkay = IsAnalyticsPage(contents, "Presence");
        }

        if (!isOkay)
        {
            if (isLoggedOut)
            {
                result?.AddError(new BaseError(ErrorCode.LoggedOut, "Your session expired. "));
            }
            else
            {
                result?.AddError(new BaseError(ErrorCode.InvalidPage.Generic, "Loaded page is wrong. "));
            }
        }

        return isOkay;
    }

    private bool ParsePageResult(string contents, PageResult result)
    {
        var identifyJson = Regex.Match(contents, @"analytics\.identify\('(\d+)',\s+\{(.*?)\}\);", RegexOptions.Singleline);
        if (identifyJson.Success)
        {
            // 2023-07-03: this things is no more available
            // {
            //    "email": "xxxxxxx@xxxxxxxxx.xxx", "name": "Xxxxxx XXXXXXXX",
            //    "firstName": "Xxxxxx",         "lastName": "XXXXXXXX",
            //    "gender": "female",             "birthday": "XXXX-XX-XX",
            //    "address": { "street": "xx xxxxxx xxxxxxxxxxxxxx",  "postalCode": "XXXXX",
            //        "city": "Xxxxxx", "country": "XX"
            //    },
            //    "company": {
            //        "id": "XXXXX",          "name": "Xxxxxxxxx",
            //        "employee_count": "XXX", "plan": "unlimited"
            //    },
            //    "createdAt": "XXXX-XX-XXTXX:XX:XX+XX:XX"
            // },
            result.UserId = identifyJson.Groups[1].Value;
            result.Identity = (JObject)JsonConvert.DeserializeObject("{" + identifyJson.Groups[2].Value + "}");
        }
        else if ((identifyJson = Regex.Match(contents, """Sentry\.setUser\((\{.+\})\);""")).Success)
        {
            // Sentry.setUser({"username":"xxxxxxx@xxxxxxxxx.com","roles":["ROLE_USER_MT","ROLE_MANAGER_MT","XXXXXXXXXXXX"]});
            var json = (JObject)JsonConvert.DeserializeObject(identifyJson.Groups[1].Value);
            result.Identity = new JObject();
            result.Identity["email"] = json.Value<string>("username");
            result.Identity["name"] = null;
            result.Identity["firstName"] = null;
            result.Identity["lastName"] = null;
            result.Identity["gender"] = null;
            result.Identity["birthday"] = null;
            result.Identity["address"] = null;
            result.Identity["company"] = null;
            result.Identity["createdAt"] = null;
            result.Identity["roles"] = json["roles"];
        }
        else
        {
            result.AddError(new BaseError(ErrorCode.PageParseMissIdentity, "Failed to extract user information. "));
        }

        var groupJson = Regex.Match(contents, @"analytics\.group\(""(\d+)"",\s+\{(.*?)\}\)", RegexOptions.Singleline);
        if (groupJson.Success)
        {
            // 2023-07-03: this things is no more available
            result.GroupId = groupJson.Groups[1].Value;
            result.Group = (JObject)JsonConvert.DeserializeObject("{" + groupJson.Groups[2].Value + "}");
        }
        else
        {
            // 2023-07-03: this things is no more available
            ////result.AddError(new BaseError(ErrorCode.PageParseMissGroup, "Failed to extract group information. "));
        }

        return result.Succeed;
    }

    // analytics.page(""security_login"")
    private static readonly Regex analyticsPageNameRegex2 = new Regex("""analytics\.page\(\s*"([^"]+)"\s*\s*\)""");

    // amplitude.track("Page View", {page: "security_login"});
    private static readonly Regex analyticsPageNameRegex3 = new Regex("""amplitude\.track\(\s*"Page View"\s*,\s*\{\s*page\s*:\s*"([^"]+)"\s*\}\s*\)""");
    
    private static bool IsAnalyticsPage(string contents, string pageName)
    {
        Match match;
        if ((match = analyticsPageNameRegex3.Match(contents)).Success)
        {
            return pageName.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        }
        else if ((match = analyticsPageNameRegex2.Match(contents)).Success)
        {
            return pageName.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            return false;
        }
    }
}
