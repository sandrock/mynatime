namespace Mynatime.Infrastructure;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

/// <summary>
/// Serializable object representing an online Manatime profile. 
/// </summary>
public sealed class MynatimeProfile : JsonObject
{
    private const string ManifestValue = "MynatimeProfile";
    private const string ManifestKey = "__manifest";

    private MynatimeProfileData? data;
    private MynatimeProfileTransaction? transaction;
    private MynatimeProfileTransaction? commits;

    public MynatimeProfile()
        : base("Root", new JObject())
    {
        this.Element[ManifestKey] = ManifestValue;
        this.Element["DateCreated"] = DateTime.UtcNow.ToString("o");
    }

    public MynatimeProfile(JObject node)
        : base("Root", node)
    {
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
        get => this.Element.Value<string>("LoginUsername");
        set => this.Element["LoginUsername"] = value;
    }

    /// <summary>
    /// Gets or sets the authentication password. 
    /// </summary>
    public string? LoginPassword
    {
        get => this.Element.Value<string>("LoginPassword");
        set => this.Element["LoginPassword"] = value;
    }

    public JArray? Cookies
    {
        get => this.Element["Cookies"] as JArray;
        set => this.Element["Cookies"] = value;
    }

    public string? UserId 
    {
        get => this.Element.Value<string>("UserId");
        set => this.Element["UserId"] = value;
    }
    public string? GroupId 
    {
        get => this.Element.Value<string>("GroupId");
        set => this.Element["GroupId"] = value;
    }
    public JObject? Identity 
    {
        get => this.Element["Identity"] as JObject;
        set => this.Element["Identity"] = value;
    }
    public JObject? Group 
    {
        get => this.Element["Group"] as JObject;
        set => this.Element["Group"] = value;
    }

    /// <summary>
    /// Contains cached domain data for the current profile. 
    /// </summary>
    public MynatimeProfileData? Data
    {
        get
        {
            if (this.data != null)
            {
            }
            else if (this.Element.TryGetValue("Data", out JToken? child))
            {
                this.data = new MynatimeProfileData((JObject)child);
            }
            else if (!this.IsFrozen)
            {
                this.data = new MynatimeProfileData(new JObject());
                this.Element.Add("Data", this.data.Element);
            }

            return this.data;
        }
    }

    /// <summary>
    /// Contains pending actions to the service.
    /// </summary>
    public MynatimeProfileTransaction? Transaction
    {
        get
        {
            if (this.transaction != null)
            {
            }
            else if (this.Element.TryGetValue("Transaction", out JToken? child))
            {
                this.transaction = new MynatimeProfileTransaction((JObject)child);
            }
            else if (!this.IsFrozen)
            {
                this.transaction = new MynatimeProfileTransaction(new JObject());
                this.Element.Add("Transaction", this.transaction.Element);
            }

            return this.transaction;
        }
    }

    /// <summary>
    /// Contains past actions to the service.
    /// </summary>
    public MynatimeProfileTransaction? Commits
    {
        get
        {
            if (this.commits != null)
            {
            }
            else if (this.Element.TryGetValue("Commits", out JToken? child))
            {
                this.commits = new MynatimeProfileTransaction((JObject)child);
            }
            else if (!this.IsFrozen)
            {
                this.commits = new MynatimeProfileTransaction(new JObject());
                this.Element.Add("Commits", this.commits.Element);
            }

            return this.commits;
        }
    }

    public bool? IsDefault
    {
        get => this.Element.Value<bool?>("IsDefault");
        set => this.Element["IsDefault"] = value;
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
        await File.WriteAllTextAsync(path, this.Element.ToString(Formatting.Indented), Encoding.UTF8);
    }

    public override string ToString()
    {
        return nameof(MynatimeProfile) + " " + (this.LoginUsername ?? "???)");
    }
}
