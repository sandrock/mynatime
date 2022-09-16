namespace MynatimeClient;

using Mynatime.Infrastructure;
using System.Net;
using System.Text.RegularExpressions;

public class NewActivityItemPage : BaseResult
{
    public bool IsEmptyCategoryAllowed { get; set; }
    
    public List<SelectItem> Categories { get; set; }

    public string Token { get; set; }

    public DateTime LoadTime { get; set; }

    public void ReadPage(string contents)
    {
        var isOk = true;
        this.LoadTime = DateTime.UtcNow;

        // read categories
        // <select id="create_task" name="create[task]" class="select2-allow-clear form-control"><option value=""></option><option value="1234">Absence xxx</option><option value="1235">Xxxxx</option><option value="1236">Interne</option>...</select>
        var regex = new Regex(@"<select id=""create_task"" name=""create\[task\]"" class=""[^""]+"">(.*)</select>");
        var selectMatch = regex.Match(contents);
        this.Categories = new List<SelectItem>();
        if (selectMatch.Success)
        {
            regex = new Regex(@"<option value=""([^""]*)"">([^<>]*)</option>");
            int i = 0;
            foreach (Match optionMatch in regex.Matches(selectMatch.Groups[1].Value))
            {
                var id = WebUtility.HtmlDecode(optionMatch.Groups[1].Value);
                var name = WebUtility.HtmlDecode(optionMatch.Groups[2].Value);
                var index = i++;
                if (string.IsNullOrEmpty(id))
                {
                    this.IsEmptyCategoryAllowed = true;
                    continue;
                }

                this.Categories.Add(new SelectItem() { Id = id, DisplayName = name, Index = i, });
            }
        }
        else
        {
            this.AddError(new BaseError("InvalidPage/MissingCategories", "The webpage is not valid. "));
        }

        // <input type="hidden" id="create__token" name="create[_token]" value="xxxxxxxxxxxxxxxxxxxxxxxxx" />
        regex = new Regex(@"<input type=""hidden"" id=""create__token"" name=""create\[_token\]"" value=""([^""]+)"" />");
        var tokenMatch = regex.Match(contents);
        if (tokenMatch.Success)
        {
            this.Token = tokenMatch.Groups[1].Value;
        }
        else
        {
            this.AddError(new BaseError("InvalidPage/MissingToken", "The webpage is not valid. "));
        }
    }
}

public class SelectItem
{
    public string DisplayName { get; set; }

    public string Id { get; set; }

    public int Index { get; set; }

    public void Update(MynatimeProfileDataActivityCategory match, DateTime time)
    {
        match.LastUpdated = time;
        match.Name = this.DisplayName;
    }
}
