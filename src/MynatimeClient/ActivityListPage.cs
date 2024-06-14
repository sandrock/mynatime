
namespace Mynatime.Client;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Mynatime.Infrastructure;

public sealed class ActivityListPage : BaseResult
{
    private static readonly Regex dualTimeRegex = new Regex("""(\d\d?):(\d\d?).*(\d\d?):(\d\d?)""", RegexOptions.Compiled);
    private static readonly Regex presenceHrefEditRegex = new Regex("""^/?presences/(\d+)/edit$""", RegexOptions.Compiled);
    
    private readonly WebForm form = new WebForm(
        "get",
        "presences/search",
        new [] { "users[]", "service", "job", "task", "dateStart", "dateEnd", "states[]", });

    public DateTime LoadTime { get; set; }

    public List<SelectItem> AvailableUsers { get; } = new();
    public List<SelectItem> AvailableServices { get; } = new();
    public List<SelectItem> AvailableJobs { get; } = new();
    public List<SelectItem> AvailableTasks { get; } = new();

    public DateTime? DateStart
    {
        get { return this.form.GetDateTimeValue("dateStart", ClientConstants.DateInputFormat, DateTimeKind.Local); }
        set { this.form.SetDateTimeValue("dateStart", value, ClientConstants.DateInputFormat); }
    }

    public DateTime? DateEnd
    {
        get { return this.form.GetDateTimeValue("dateEnd", ClientConstants.DateInputFormat, DateTimeKind.Local); }
        set { this.form.SetDateTimeValue("dateEnd", value, ClientConstants.DateInputFormat); }
    }

    public void ReadPage(string contents)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(contents);
        
        // <div class="content flex-grow-1">
        var main = doc.DocumentNode.SelectNodes("div").Where(x => x.HasClass("content")).Single();
        
        // this one: <form class="dropdown-menu dropdown-filter dropdown-keep-open px-4 py-3" action="/presences/search">
        // not this: <form action="/presences/update/process-multiple" method="post">
        var mainForm = main.SelectNodes("form").Where(x => "/precences/saerch".Equals(x.GetAttributeValue("action", string.Empty))).Single();
        
        // <select name="users[]" id="users" class="form-control" multiple="multiple" style="width: 100%!important;">
        // <option value=""></option>
        // <option value="123456">Firstname Lastname</option>
        // <option value="456789" selected="selected">Firstname Lastname</option>
        var usersSelect = mainForm.SelectNodes("select").Where(x => "users[]".Equals(x.GetAttributeValue("name", string.Empty))).Single();
        this.AvailableUsers.AddRange(usersSelect.ChildNodes
           .Where(x => !string.IsNullOrEmpty(x.GetAttributeValue("value", null)))
           .Select(x => new SelectItem(x.GetAttributeValue("value", null), x.InnerText)));
        var selectedUsers = usersSelect.ChildNodes
           .Where(x => !string.IsNullOrEmpty(x.GetAttributeValue("value", null)))
           .Where(x => x.Attributes.Contains("selected"))
           .Select(x => x.GetAttributeValue("value", null));
        foreach (var userId in selectedUsers)
        {
            this.form.AddStringValue("users[]", userId);
        }
        
        // <select name="service" id="service" class="form-control select2-allow-clear" style="width: 100%!important;">
        // <option value=""></option>
        // <option value="123456">Service XXX</option>
        var serviceSelect = mainForm.SelectNodes("select").Where(x => "service".Equals(x.GetAttributeValue("name", string.Empty))).Single();
        this.AvailableServices.AddRange(serviceSelect.ChildNodes
           .Where(x => !string.IsNullOrEmpty(x.GetAttributeValue("value", null)))
           .Select(x => new SelectItem(x.GetAttributeValue("value", null), x.InnerText)));
        
        // <select name="job" id="job" class="form-control select2-allow-clear" style="width: 100%!important;">
        // <option value=""></option>
        // <option value="123456">Job XXX</option>
        var jobSelect = mainForm.SelectNodes("select").Where(x => "job".Equals(x.GetAttributeValue("name", string.Empty))).Single();
        this.AvailableJobs.AddRange(jobSelect.ChildNodes
           .Where(x => !string.IsNullOrEmpty(x.GetAttributeValue("value", null)))
           .Select(x => new SelectItem(x.GetAttributeValue("value", null), x.InnerText)));
        
        // <select name="task" id="task" class="form-control select2-allow-clear" style="width: 100%!important;">
        // <option value=""></option>
        // <option value="123456">Activity XXX</option>
        var taskSelect = mainForm.SelectNodes("select").Where(x => "task".Equals(x.GetAttributeValue("name", string.Empty))).Single();
        this.AvailableTasks.AddRange(taskSelect.ChildNodes
           .Where(x => !string.IsNullOrEmpty(x.GetAttributeValue("value", null)))
           .Select(x => new SelectItem(x.GetAttributeValue("value", null), x.InnerText)));
        
        // <input type="text" class="form-control input-sm " name="dateStart" value="2024-05-07" placeholder="aaaa-mm-jj" autocomplete="off">
        var dateStartInput = mainForm.SelectNodes("input").Where(x => "dateStart".Equals(x.GetAttributeValue("name", string.Empty))).Single();
        this.form.SetStringValue("dateStart", dateStartInput.GetAttributeValue("value", null));
        
        // <input type="text" class="input-sm form-control" name="dateEnd" value="2024-05-31" placeholder="aaaa-mm-jj" autocomplete="off">
        var dateEndInput = mainForm.SelectNodes("input").Where(x => "dateEnd".Equals(x.GetAttributeValue("name", string.Empty))).Single();
        this.form.SetStringValue("dateEnd", dateEndInput.GetAttributeValue("value", null));
        
        // <select id="states" name="states[]" class="form-control select2-allow-clear" multiple="multiple" style="width: 100%!important;">
        // <option value=""></option>
        // <option value="9" >En cours</option>
        // <option value="2" >En attente</option>
        // <option value="1" >Validé</option>
        var statesSelect = mainForm.SelectNodes("select").Where(x => "states[]".Equals(x.GetAttributeValue("name", string.Empty))).Single();

        // <table class="table table-striped">
        // <thead>...</thead>
        // <tbody>...</tbody>
        var dataTable = main.SelectSingleNode("table");
        var dataBody = dataTable.SelectSingleNode("tbody");
        foreach (var row in dataBody.ChildNodes) // <tr>
        {
            var enumerator = row.ChildNodes.Select(x => x).GetEnumerator();
            HtmlNode cell;
            
            // user
            enumerator.MoveNext();
            cell = enumerator.Current;
            var userId = cell.SelectSingleNode("input").GetAttributeValue("value", null);
            var userDisplayName = cell.SelectSingleNode("label").InnerText;
            
            // date
            enumerator.MoveNext();
            cell = enumerator.Current;
            var date = DateTime.ParseExact(cell.InnerText.Trim(), ClientConstants.DateInputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None)
               .ChangeKind(DateTimeKind.Unspecified);
            
            // time start and end
            // """  14:00 <i></i> 17:00 """
            enumerator.MoveNext();
            cell = enumerator.Current;
            var match = dualTimeRegex.Match(cell.InnerText);
            var timeStart = TimeSpan.FromHours(int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture))
               .Add(TimeSpan.FromMinutes(int.Parse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture)));
            var timeEnd = TimeSpan.FromHours(int.Parse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture))
               .Add(TimeSpan.FromMinutes(int.Parse(match.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture)));
            
            // actitité
            enumerator.MoveNext();
            cell = enumerator.Current;
            var categoryName = cell.InnerText;
            
            // state
            enumerator.MoveNext();
            cell = enumerator.Current;
            
            // comment
            enumerator.MoveNext();
            cell = enumerator.Current;

            // actions
            enumerator.MoveNext();
            cell = enumerator.Current;
            var link = cell.SelectSingleNode("a").GetAttributeValue("href", null);
            var id = presenceHrefEditRegex.Match(link).Groups[0].Value;
            
            
        }
    }
}
