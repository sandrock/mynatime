namespace Mynatime.Infrastructure;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

/// <summary>
/// Serializable object representing an online Manatime profile. 
/// </summary>
public sealed class MynatimeProfile : JObject
{
    private const string ManifestValue = "MynatimeProfile";
    private const string ManifestKey = "__manifest";

    private readonly JObject element;

    public MynatimeProfile()
    {
        this.element = new JObject();
        this.element[ManifestKey] = ManifestValue;
        this.element["DateCreated"] = DateTime.UtcNow.ToString("o");
    }

    public MynatimeProfile(JObject node)
    {
        this.element = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>
    /// Gets the file path the profile has been loaded from/to. 
    /// </summary>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Gets or sets the authentication username/email. 
    /// </summary>
    public string? LoginUsername
    {
        get => this.element.Value<string>("LoginUsername");
        set => this.element["LoginUsername"] = value;
    }

    /// <summary>
    /// Gets or sets the authentication password. 
    /// </summary>
    public string? LoginPassword
    {
        get => this.element.Value<string>("LoginPassword");
        set => this.element["LoginPassword"] = value;
    }

    public JArray? Cookies
    {
        get => this.element["Cookies"] as JArray;
        set => this.element["Cookies"] = value;
    }

    public string? UserId 
    {
        get => this.element.Value<string>("UserId");
        set => this.element["UserId"] = value;
    }
    public string? GroupId 
    {
        get => this.element.Value<string>("GroupId");
        set => this.element["GroupId"] = value;
    }
    public JObject? Identity 
    {
        get => this.element["Identity"] as JObject;
        set => this.element["Identity"] = value;
    }
    public JObject? Group 
    {
        get => this.element["Group"] as JObject;
        set => this.element["Group"] = value;
    }

    /// <summary>
    /// Loads the profile from a JSON file. 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">invalid document</exception>
    public static async Task<MynatimeProfile> LoadFromFile(string path)
    {
        var contents = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var root = (JObject)JsonConvert.DeserializeObject(contents);

        if (ManifestValue != root.Value<string>(ManifestKey))
        {
            throw new InvalidOperationException("File is not a " + nameof(MynatimeProfile) + ". ");
        }

        var me = new MynatimeProfile(root);
        me.FilePath = path;
        return me;
    }

    /// <summary>
    /// Stores the profile to a JSON file. 
    /// </summary>
    /// <param name="path"></param>
    public async Task SaveToFile(string path)
    {
        this.FilePath = path;
        await File.WriteAllTextAsync(path, this.element.ToString(Formatting.Indented), Encoding.UTF8);
    }

    public override string ToString()
    {
        return nameof(MynatimeProfile) + " " + (this.LoginUsername ?? "???)");
    }
}
