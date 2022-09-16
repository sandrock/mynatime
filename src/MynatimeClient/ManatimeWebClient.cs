namespace MynatimeClient;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
                    return this.Log(BaseResult.Error<BaseResult>("PageParseMiss/CSRF", "Authentication prepare failed. "));
                }
            }
            else
            {
                return this.Log(BaseResult.Error<LoginResult>("UnknownError", "Other error. "));
            }
        }
        catch (Exception ex)
        {
            this.Log(nameof(this.PrepareEmailPasswordAuthenticate), ex);
            return this.Log(BaseResult.Error<BaseResult>("UnknownError", "Authentication prepare failed. "));
        }
    }

    public async Task<LoginResult> EmailPasswordAuthenticate(string username, string password)
    {
        if (!this.canEmailPasswordAuthenticate || this.csrfToken == null)
        {
            return BaseResult.Error<LoginResult>("MissingCsrfToken", "Cannot authenticate with the CSRF token. ");
        }
        /*
<form action="/security/login_check" method="post">
    <input type="hidden"                    name="_csrf_token"   value="kCCyMAVBpsQjea3O4vgnyHOpERWK54mB3pJZH1kBGQQ"/>
    <input type="text" id="username"        name="_username"     value="" required="required" autofocus="autofocus"/>
    <input type="password" id="password"    name="_password"     required="required"/>
    <input type="checkbox" id="remember_me" name="_remember_me"  value="on"/>&nbsp;Se souvenir de moi
    <button type="submit" class="btn btn-primary ml-3" id="_submit" name="_submit">
</form>
         */
        var request = this.CreateRequest(HttpMethod.Post, "security/login_check");
        var requestData = new List<KeyValuePair<string, string>>();
        requestData.Add(new KeyValuePair<string, string>("_csrf_token", this.csrfToken));
        requestData.Add(new KeyValuePair<string, string>("_username", username));
        requestData.Add(new KeyValuePair<string, string>("_password", password));
        requestData.Add(new KeyValuePair<string, string>("remember_me=", "on"));
        requestData.Add(new KeyValuePair<string, string>("_submit=", ""));
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
                result.AddError(new BaseError("InvalidUsernameOrPassword", "Invalid credentials. "));
                return this.Log(result);
            }

            this.ParsePageResult(contents, result);

            return this.Log(result);
        }
        else
        {
            return this.Log(BaseResult.Error<LoginResult>("UnknownError", "Other error. "));
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
            return this.Log(BaseResult.Error<HomeResult>("UnknownError", "Other error. "));
        }
    }

    public async Task<NewActivityItemPage> GetNewActivityItemPage()
    {
        var request = this.CreateRequest(HttpMethod.Get, "presences/create/advanced");
        var response = await this.Send(nameof(GetNewActivityItemPage), request);
        if (response.IsSuccessStatusCode)
        {
            var result = new NewActivityItemPage();
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
            return this.Log(BaseResult.Error<NewActivityItemPage>("UnknownError", "Other error. "));
        }
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
        bool isOkay = false, isLoggedOut = false;
        if (contents.Contains(@"analytics.page(""security_login"");"))
        {
            isLoggedOut = true;
            isOkay = desiredPage == ManatimePage.SecurityLogin;
        }

        if (desiredPage == ManatimePage.Home)
        {
            isOkay = contents.Contains("analytics.page(\"legacy_home\");");
        }
        else if (desiredPage == ManatimePage.PresenceCreateAdvanced)
        {
            isOkay = contents.Contains("analytics.page(\"presence_create_advanced\");");
        }

        if (!isOkay && isLoggedOut)
        {
            result?.AddError(new BaseError("LoggedOut", "Your session expired. "));
        }
        else if (!isOkay)
        {
            result?.AddError(new BaseError("InvalidPage", "Loaded page is wrong. "));
        }

        return isOkay;
    }

    private bool ParsePageResult(string contents, PageResult result)
    {
        var identifyJson = Regex.Match(contents, @"analytics\.identify\('(\d+)',\s+\{(.*?)\}\);", RegexOptions.Singleline);
        if (identifyJson.Success)
        {
            result.UserId = identifyJson.Groups[1].Value;
            result.Identity = (JObject)JsonConvert.DeserializeObject("{" + identifyJson.Groups[2].Value + "}");
        }
        else
        {
            result.AddError(new BaseError("PageParseMiss/Identity", "Failed to extract user information. "));
        }
            
        var groupJson = Regex.Match(contents, @"analytics\.group\('(\d+)',\s+\{(.*?)\}\)", RegexOptions.Singleline);
        if (identifyJson.Success)
        {
            result.GroupId = groupJson.Groups[1].Value;
            result.Group = (JObject)JsonConvert.DeserializeObject("{" + groupJson.Groups[2].Value + "}");
        }
        else
        {
            result.AddError(new BaseError("PageParseMiss/Group", "Failed to extract group information. "));
        }

        return result.Succeed;
    }
}
