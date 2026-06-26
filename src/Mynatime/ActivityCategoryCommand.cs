
namespace Mynatime.CLI;

using Fastenshtein;
using Mynatime.Client;
using Mynatime.Infrastructure;
using SimplifiedSearch;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

public sealed class ActivityCategoryCommand : Command
{
    private readonly IManatimeWebClient manatimeWebClient;

    public ActivityCategoryCommand(IConsoleApp consoleApp, IManatimeWebClient manatimeWebClient, IAnsiConsole console)
        : base(consoleApp, console)
    {
        this.manatimeWebClient = manatimeWebClient;
    }

    public static string[] Args { get; } = new string[] { "cat", "cate", "categ", "category", "categories", };

    public bool DoRefresh { get; set; } = false;

    public string? Search { get; set; }

    public string? Alias { get; set; }

    public string? Unalias { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Activity categories";
        var prefix = ActivityCommand.Args[0] + " " + Args[0] + " ";
        describe.AddCommandPattern(prefix, "lists activity categories");
        describe.AddCommandPattern(prefix + "refresh", "updates activity categories from service");
        describe.AddCommandPattern(prefix + "search <q>", "searches categories");
        describe.AddCommandPattern(prefix + "alias <name> <alias>", "creates an alias for a category");
        describe.AddCommandPattern(prefix + "unalias <alias>", "removes an alias from a category");
        return describe;
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ActivityCommand.Args);
    }

    public override bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command)
    {
        var i = -1;
        if (++i >= args.Length || !this.MatchArg(args[i]))
        {
            goto error;
        }

        if (++i >= args.Length || !ConsoleApp.MatchArg(args[i], Args))
        {
            goto error;
        }

        bool isSearching = false;
        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArgs = new string[args.Length - i - 1];
            Array.Copy(args, i + 1, nextArgs, 0, args.Length - i - 1);

            string? value = null;
            if (ConsoleApp.MatchArg(arg, "--refresh", "refresh") || ConsoleApp.MatchShortArg(arg, "-r"))
            {
                this.DoRefresh = true;
            }
            else if (ConsoleApp.MatchArg(arg, "search") || ConsoleApp.MatchShortArg(arg, "-q", out value))
            {
                if (value != null)
                {
                    isSearching = true;
                    this.Search = value;
                }
                else if (nextArgs.Length >= 1)
                {
                    isSearching = true;
                    this.Search = nextArgs[0];
                    i++;
                }
                else
                {
                    this.Console.WriteLine("Search command requires a search expression. ");
                }
            }
            else if (ConsoleApp.MatchArg(arg, "alias"))
            {
                if (nextArgs.Length >= 2)
                {
                    this.Search = nextArgs[0];
                    this.Alias = nextArgs[1];
                    i++;
                    i++;
                }
                else
                {
                    this.Console.WriteLine("Alias command requires two values. ");
                }
            }
            else if (ConsoleApp.MatchArg(arg, "unalias"))
            {
                if (nextArgs.Length >= 1)
                {
                    this.Unalias = nextArgs[0];
                    i++;
                }
                else
                {
                    this.Console.WriteLine("Unalias command requires an alias value. ");
                }
            }
            else
            {
                if (isSearching)
                {
                    // capture more search terms
                    // TODO: store each search arg and code incremental search?
                    this.Search += " " + arg;
                }
                else
                {
                    goto error;
                }
            }
        }

        ////ok:
        consumedArgs = args.Length;
        command = this;
        return true;

        error:
        consumedArgs = 0;
        command = null;
        return false;
    }

    public override async Task Run()
    {
        var profile = this.App.CurrentProfile;

        if (profile == null)
        {
            this.Console.MarkupLine("[red]No current profile.[/]");
            return;
        }

        var store = profile.Data?.ActivityCategories!;
        var existingItems = store.Items.ToList();
        var newItems = new List<MynatimeProfileDataActivityCategory>();
        var deletedItems = new List<MynatimeProfileDataActivityCategory>();
        var changedItems = new List<MynatimeProfileDataActivityCategory>();

        // refresh from service
        bool hasRefreshed = false, hasChanged = false;
        if (this.DoRefresh)
        {
            hasRefreshed = await this.Refresh(profile, existingItems, newItems, deletedItems, changedItems);
        }

        // read store and display items
        if (store != null)
        {
            var allItems = store.Items.ToList();
            IList<MynatimeProfileDataActivityCategory> items = allItems;
            if (this.Search != null)
            {
                var searchResult = await SearchItems(allItems, this.Search, this.Alias != null);
                items = searchResult
                   .Select(x => x.Item)
                   .ToList();
            }

            if (this.Alias != null)
            {
                if (items.Count == 0)
                {
                    this.Console.MarkupLine($"[red]Create alias: no such activity \"{Markup.Escape(this.Search!)}\"[/]");
                }
                else if (items.Count > 1)
                {
                    this.Console.MarkupLine($"[yellow]Create alias: too many activities for \"{Markup.Escape(this.Search!)}\"[/]");
                }
                else
                {
                    hasChanged = true;
                    var item = items[0];
                    item.Alias = this.Alias.Trim();
                    this.Console.MarkupLine($"[green]Create alias:[/] [{CliTheme.CategoryAlias}]\"{Markup.Escape(item.Alias)}\"[/] set on [{CliTheme.Category}]\"{Markup.Escape(item.Name ?? string.Empty)}\"[/] ({Markup.Escape(item.Id ?? string.Empty)})");
                }
            }

            if (this.Unalias != null)
            {
                var target = this.Unalias.Trim();
                var matches = allItems
                   .Where(x => x.Alias != null && string.Equals(x.Alias.Trim(), target, StringComparison.OrdinalIgnoreCase))
                   .ToList();
                if (matches.Count == 0)
                {
                    this.Console.MarkupLine($"[red]Remove alias: no category with alias \"{Markup.Escape(target)}\"[/]");
                }
                else
                {
                    foreach (var item in matches)
                    {
                        hasChanged = true;
                        this.Console.MarkupLine($"[green]Remove alias:[/] [{CliTheme.CategoryAlias}]\"{Markup.Escape(item.Alias!)}\"[/] removed from [{CliTheme.Category}]\"{Markup.Escape(item.Name ?? string.Empty)}\"[/] ({Markup.Escape(item.Id ?? string.Empty)})");
                        item.Alias = null;
                    }
                }
            }

            // display list (skipped when unaliasing: the confirmation line is enough)
            if (this.Unalias == null)
            {
                var table = new Table().Border(TableBorder.Simple);
                table.AddColumn("ID");
                table.AddColumn("Name");
                table.AddColumn("Alias");
                table.AddColumn("Status");

                foreach (var item in items)
                {
                    string status;
                    if (newItems.Any(x => x.Id != null && x.Id.Equals(item.Id)))
                        status = "[green]NEW[/]";
                    else if (changedItems.Contains(item))
                        status = "[yellow]CHANGED[/]";
                    else
                        status = string.Empty;

                    table.AddRow(
                        Markup.Escape(item.Id ?? string.Empty),
                        CliTheme.Tag(CliTheme.Category, item.Name),
                        CliTheme.Tag(CliTheme.CategoryAlias, item.Alias),
                        status);
                }

                foreach (var item in deletedItems)
                {
                    table.AddRow(
                        Markup.Escape(item.Id ?? string.Empty),
                        CliTheme.Tag(CliTheme.Category, item.Name),
                        CliTheme.Tag(CliTheme.CategoryAlias, item.Alias),
                        "[red]DELETED[/]");
                }

                if (table.Rows.Count > 0)
                {
                    this.Console.Write(table);
                }
                else
                {
                    this.Console.WriteLine("No categories found.");
                }
            }
        }

        // save store if changed
        if (hasRefreshed || hasChanged)
        {
            // TODO: this is not unit-testable :@
            Debug.Assert(profile.FilePath != null, "profile.FilePath != null");
            await profile.SaveToFile(profile.FilePath);
        }
    }

    private async Task<bool> Refresh(
        MynatimeProfile profile,
        List<MynatimeProfileDataActivityCategory> existingItems,
        List<MynatimeProfileDataActivityCategory> newItems,
        List<MynatimeProfileDataActivityCategory> deletedItems,
        List<MynatimeProfileDataActivityCategory> changedItems)
    {
        var profileData = profile.Data ?? new MynatimeProfileData();

        NewActivityItemPage page = null!;
        string? refreshError = null;
        await this.Console.Status().StartAsync("Refreshing categories...", async ctx =>
        {
            page = await this.manatimeWebClient.GetNewActivityItemPage();
            if (page.Succeed)
            {
                // yeah!
            }
            else if (page.Errors?.Any(x => x.Code == ErrorCode.LoggedOut) ?? false)
            {
                if (profile.LoginUsername == null || profile.LoginPassword == null)
                {
                    refreshError = "Missing authentication information.";
                    return;
                }

                ctx.Status("Renewing session...");
                var loginPage = await this.manatimeWebClient.PrepareEmailPasswordAuthenticate();
                if (loginPage.Succeed)
                {
                    var loginResultPage = await this.manatimeWebClient.EmailPasswordAuthenticate(profile.LoginUsername, profile.LoginPassword);
                    if (loginResultPage.Succeed)
                    {
                        ctx.Status("Refreshing categories...");
                        page = await this.manatimeWebClient.GetNewActivityItemPage();
                        if (!page.Succeed)
                        {
                            refreshError = "Auto log-in failed: something went wrong 3.";
                        }
                    }
                    else
                    {
                        refreshError = "Auto log-in failed: something went wrong 2.";
                    }
                }
                else
                {
                    refreshError = "Auto log-in failed: something went wrong 1.";
                }
            }
            else
            {
                refreshError = "Auto log-in failed: something went wrong 0.";
            }
        });

        if (refreshError != null)
        {
            this.Console.MarkupLine("[red]" + Markup.Escape(refreshError) + "[/]");
        }

        profile.Cookies = this.manatimeWebClient.GetCookies();

        if (page.Succeed)
        {
            // add and update items 
            foreach (var category in page.Categories)
            {
                var match = existingItems.SingleOrDefault(x => x.Id!.Equals(category.Id, StringComparison.Ordinal));
                if (match != null)
                {
                    match.LastUpdated = page.LoadTime;
                    category.UpdateFrom(match, page.LoadTime!.Value);
                    changedItems.Add(match);
                }
                else
                {
                    match = new MynatimeProfileDataActivityCategory(category.Id, category.DisplayName);
                    match.Created = page.LoadTime;
                    category.UpdateFrom(match, page.LoadTime!.Value);
                    newItems.Add(match);
                }
            }

            // remove items
            foreach (var entry in existingItems)
            {
                var match = page.Categories.SingleOrDefault(x => x.Id!.Equals(entry.Id, StringComparison.Ordinal));
                if (match != null)
                {
                    if (entry.Deleted != null)
                    {
                        entry.Deleted = null;
                    }
                }
                else
                {
                    if (entry.Deleted == null)
                    {
                        deletedItems.Add(entry);
                        entry.Deleted = page.LoadTime;
                    }
                }
            }

            // insert new items
            Debug.Assert(profileData.ActivityCategories != null, "profileData.ActivityCategories != null");
            foreach (var entry in newItems)
            {
                profileData.ActivityCategories.Add(entry);
            }

            profileData.ActivityCategories.LastUpdated = page.LoadTime;

            return true;
        }
        else
        {
            this.Console.MarkupLine("[red]" + Markup.Escape(page.ToString()!) + "[/]");
            return false;
        }
    }

    public static async Task<IList<SearchResultItem<MynatimeProfileDataActivityCategory>>> SearchItems(List<MynatimeProfileDataActivityCategory> source, string search, bool findBest)
    {
        var result = new List<SearchResultItem<MynatimeProfileDataActivityCategory>>();

        var searchResult = await source.SimplifiedSearchAsync(search, x => x.Name);

        foreach (var item in searchResult)
        {
            result.Add(new SearchResultItem<MynatimeProfileDataActivityCategory>(item));
        }

        if (findBest && result.Count > 1)
        {
            var levensteins = new Dictionary<SearchResultItem<MynatimeProfileDataActivityCategory>, int>(result.Count);
            for (var i = 0; i < result.Count; i++)
            {
                var item = result[i];
                var name = item.Item.Name;
                if (name != null)
                {
                    levensteins[item] = Levenshtein.Distance(search.ToUpperInvariant(), name.ToUpperInvariant());
                }
            }

            var sorted = levensteins.OrderBy(x => x.Value).ToArray();
            var distancesBetweenDistances = new int[levensteins.Count];
            int previousValue = 0;
            for (var i = 0; i < sorted.Length; i++)
            {
                var item = sorted[i];
                distancesBetweenDistances[i] = item.Value - previousValue;
                previousValue = item.Value;
            }

            if (distancesBetweenDistances[0] < distancesBetweenDistances[1])
            {
                // first one looks better than the others
                var first = sorted.First().Key;
                result.RemoveAll(x => x != first);
            }
        }
        
        /*
        // exact match search
        foreach (var item in source)
        {
            if (item.Id != null && item.Id.Equals(search, StringComparison.OrdinalIgnoreCase))
            {
                result.AddIfAbsent(item);
            }
            
            if (item.Name != null && item.Name.Equals(search, StringComparison.OrdinalIgnoreCase))
            {
                result.AddIfAbsent(item);
            }
        }
        
        // partial match search foreach (var item in source)
        foreach (var item in source)
        {
            if (item.Name != null && item.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                result.AddIfAbsent(item);
            }
        }
        */

        return result;
    }

    public sealed class SearchResultItem<T>
        where T : class
    {
        public SearchResultItem([DisallowNull] T item)
        {
            this.Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public T Item { get; }

        public override string ToString()
        {
            return this.Item.ToString()!;
        }
    }
}
