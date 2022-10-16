namespace Mynatime.Client;

using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public sealed class NewActivityItemPage : BaseResult, ITransactionItem
{
    const string TransactionObjectType = "Mynatime.Client.NewActivityItemPage";

    private readonly WebForm form = new WebForm("create[task]", "create[dateStart]", "create[dateEnd]", "create[inAt]", "create[outAt]", "create[duration]", "create[comment]", "submitAdvanced", "create[_token]");

    static NewActivityItemPage()
    {
        MynatimeProfileTransactionManager.Default.RegisterTransactionItemType<NewActivityItemPage>(
            TransactionObjectType,
            x => new NewActivityItemPage(x));
    }

    public NewActivityItemPage()
    {
    }

    private NewActivityItemPage(MynatimeProfileTransactionItem item)
    {
        var formData = item.Element.Value<string>("FormData");
        this.form.LoadFormData(formData);
    }

    /*
     Accept-Language: en-GB,en;q=0.5
     Content-Type: application/x-www-form-urlencoded
    POST presences/create/advanced
         create%5Btask%5D=12345
        &create%5BdateStart%5D=2022-09-20
        &create%5BdateEnd%5D=2022-09-20
        &create%5BinAt%5D=22%3A21
        &create%5BoutAt%5D=22%3A22
        &create%5Bduration%5D=
        &create%5Bcomment%5D=comment%0D%0Acomment+comment%0D%0Acomment
        &submitAdvanced=
        &create%5B_token%5D=xuy888xuy888IIIIIIIIIIII-uUYuYUyuy88899_iii
    */

    public WebForm WebForm { get => this.form; }

    public DateTime? LoadTime { get; set; }

    public bool IsEmptyCategoryAllowed { get; set; }

    public List<SelectItem> Categories { get; set; }

    public string? Token
    {
        get { return this.form.GetStringValue("create[_token]"); }
        set { this.form.SetStringValue("create[_token]", value); }
    }

    public string? ActivityId
    {
        get { return this.form.GetStringValue("create[task]"); }
        set { this.form.SetStringValue("create[task]", value); }
    }

    public DateTime? DateStart
    {
        get => this.form.GetDateTimeValue("create[dateStart]", ClientConstants.DateInputFormat, DateTimeKind.Local);
        set => this.form.SetDateTimeValue("create[dateStart]", value, ClientConstants.DateInputFormat);
    }

    public DateTime? DateEnd
    {
        get => this.form.GetDateTimeValue("create[dateEnd]", ClientConstants.DateInputFormat, DateTimeKind.Local);
        set => this.form.SetDateTimeValue("create[dateEnd]", value, ClientConstants.DateInputFormat);
    }

    public TimeSpan? InAt
    {
        get => this.form.GetTimeSpanValue("create[inAt]", ClientConstants.HourMinuteTimeFormat);
        set => this.form.SetTimeSpanValue("create[inAt]", value, ClientConstants.HourMinuteTimeFormat);
    }

    public TimeSpan? OutAt
    {
        get => this.form.GetTimeSpanValue("create[outAt]", ClientConstants.HourMinuteTimeFormat);
        set => this.form.SetTimeSpanValue("create[outAt]", value, ClientConstants.HourMinuteTimeFormat);
    }

    public string? Duration
    {
        get { return this.form.GetStringValue("create[duration]"); }
        set { this.form.SetStringValue("create[duration]", value); }
    }

    public string? Comment
    {
        get { return this.form.GetStringValue("create[comment]"); }
        set { this.form.SetStringValue("create[comment]", value); }
    }

    public void ReadPage(string contents)
    {
        var isOk = true;
        this.LoadTime = DateTime.UtcNow;
        
        // locate form in page, reduce amount of string to parse
        var formStart = Regex.Match(contents, "<form +name=\"create\" +method=\"post\" +action=\"/presences/create/advanced\">");
        if (!formStart.Success)
        {
            this.AddError(new BaseError("InvalidPage/FormStartMissing"));
            return;
        }

        contents = contents.Substring(formStart.Index);
        var formEnd = Regex.Match(contents, "</form>");
        if (!formEnd.Success)
        {
            this.AddError(new BaseError("InvalidPage/FormEndMissing"));
            return;
        }

        contents = contents.Substring(0, formEnd.Index + formEnd.Length);

        // read categories
        // <select id="create_task" name="create[task]" class="select2-allow-clear form-control"><option value=""></option><option value="1234">Absence xxx</option><option value="1235">Xxxxx</option><option value="1236">Interne</option>...</select>
        var regex = new Regex(@"<select id=""create_task"" name=""create\[task\]""[^<>]*>(.*)</select>");
        var selectMatch = regex.Match(contents);
        this.Categories = new List<SelectItem>();
        if (selectMatch.Success)
        {
            regex = new Regex(@"<option[^<>]+value=""([^""]*)""[^<>]*>([^<>]*)</option>");
            int i = -1;
            foreach (Match optionMatch in regex.Matches(selectMatch.Groups[1].Value))
            {
                i++;
                var id = WebUtility.HtmlDecode(optionMatch.Groups[1].Value);
                var name = WebUtility.HtmlDecode(optionMatch.Groups[2].Value);
                if (i == 0)
                {
                    this.ActivityId = id;
                }

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
        
        // read create[dateStart]
        // <input type="text" id="create_dateStart" name="create[dateStart]" required="required" placeholder="aaaa-mm-jj" class="form-control form-control" autocomplete="off" value="2022-09-22" />
        ;

        // read create[dateEnd]
        // <input type="text" id="create_dateEnd" name="create[dateEnd]" required="required" placeholder="aaaa-mm-jj" class="form-control form-control" autocomplete="off" value="2022-09-22" />
        ;

        // read create[inAt]
        // <input type="time" id="create_inAt" name="create[inAt]" class="form-control" value="22:57" />
        ;

        // read create[outAt]
        // <input type="time" id="create_outAt" name="create[outAt]" class="form-control" value="22:58" />
        ;

        // read create[duration]
        // <input type="text" id="create_duration" name="create[duration]" class="form-control" />
        ;

        // read create[comment]
        // <textarea id="create_comment" name="create[comment]" class="form-control"></textarea>
        ;

        // read create[_token]
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

    public string GetFormData()
    {
        return this.form.GetFormData();
    }

    public MynatimeProfileTransactionItem ToTransactionItem(MynatimeProfileTransactionItem? root, DateTime utcNow)
    {
        if (root == null)
        {
            root = new MynatimeProfileTransactionItem();
            root.ObjectType = TransactionObjectType;
            root.TimeCreatedUtc = utcNow;
        }
        
        root.Element["FormData"] = this.form.GetFormData();
        return root;
    }

    public override string ToString()
    {
        return nameof(NewActivityItemPage)
            + " " + (this.LoadTime != null ? ((this.Succeed ? "OK" : ("FAIL " + this.GetErrorCode()))) : string.Empty)
          + " " + string.Join(" ", this.form.GetPairs().Select(x => x.Key + "=" + x.Value));
    }

    public string ToDisplayString(MynatimeProfileData data)
    {
        var sb = new StringBuilder();
        var sep = string.Empty;
        if (this.DateStart != null)
        {
            sb.Append(sep);
            sb.Append(this.DateStart.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
            sep = " ";
        }

        if (this.InAt != null)
        {
            sb.Append(sep);
            sb.Append(this.InAt.Value.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
            sep = " ";
        }

        if (this.DateEnd != null && (this.DateStart == null || this.DateStart.Value != this.DateEnd.Value))
        {
            sb.Append(sep);
            sb.Append(this.DateEnd.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
            sep = " ";
        }

        if (this.OutAt != null)
        {
            sb.Append(sep);
            sb.Append(this.OutAt.Value.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
            sep = " ";
        }

        if (this.Duration != null)
        {
            sb.Append(sep);
            sb.Append(this.Duration);
            sep = " ";
        }

        if (this.ActivityId != null)
        {
            var activity = data.GetActivityById(this.ActivityId);
            sb.Append(sep);
            sb.Append(activity.Name);
            sep = " ";
        }

        return sb.ToString();
    }

    public void Arrange()
    {
        if (this.DateStart == null && this.DateEnd != null)
        {
            this.DateStart = this.DateEnd;
        }
        else if (this.DateStart != null && this.DateEnd == null)
        {
            this.DateEnd = this.DateStart;
        }
        else
        {
            // lookd good
        }

        if (this.Duration != null && (this.InAt != null || this.OutAt != null))
        {
            this.AddError(new BaseError("InvalidForm/DurationOrTimes", "Cannot set both duration and times. "));
        }

        if (this.Duration != null)
        {
            if (decimal.TryParse(this.Duration, NumberStyles.Number, ClientConstants.NumberLang, out decimal value))
            {
                if (value <= 0)
                {
                    this.AddError(new BaseError("InvalidForm/Duration/Minimum", "Duration is set too low. "));
                }
                else if (value > 200)
                {
                    this.AddError(new BaseError("InvalidForm/Duration/Maximum", "Duration is set too high. "));
                }
            }
            else
            {
                this.AddError(new BaseError("InvalidForm/Duration/NotNumber", "Duration must be a number. "));
            }
        }
    }

    public DateTime? GetEndTime()
    {
        if (this.DateEnd != null && this.OutAt != null)
        {
            return this.DateEnd.Value.Add(this.OutAt.Value);
        }
        else
        {
            return null;
        }
    }
}
