namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

public class JsonObject
{
    private readonly string name;
    private readonly JObject element;
    private bool isFrozen;

    protected JsonObject(string name, JObject element)
    {
        this.name = name;
        this.element = element;
    }

    public JObject Element
    {
        get => this.element;
    }

    public bool IsFrozen { get => this.isFrozen; }

    public virtual void Freeze()
    {
        this.isFrozen = true;
    }
}
