namespace MynatimeClient;

using Newtonsoft.Json.Linq;

public class PageResult : BaseResult
{
    public string UserId { get; set; }

    public JObject Identity { get; set; }

    public string GroupId { get; set; }

    public JObject Group { get; set; }
}
