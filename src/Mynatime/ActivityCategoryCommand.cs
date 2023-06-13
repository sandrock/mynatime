
namespace Mynatime.CLI;

using Fastenshtein;
using Mynatime.Client;
using Mynatime.Infrastructure;
using SimplifiedSearch;
using System;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

public sealed class ActivityCategoryCommand : Command
{
    private readonly IManatimeWebClient manatimeWebClient;

    public ActivityCategoryCommand(IConsoleApp consoleApp, IManatimeWebClient manatimeWebClient)
        : base(consoleApp)
    {
        this.manatimeWebClient = manatimeWebClient;
    }

    public static string[] Args { get; } = new string[] { "cat", "cate", "categ", "category", "categories", };

    public bool DoRefresh { get; set; } = false;

    public string? Search { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        var prefix = ActivityCommand.Args[0] + " " + Args[0] + " ";
        describe.AddCommandPattern(prefix, "lists activity categories");
        describe.AddCommandPattern(prefix + "refresh", "updates activity categories from service");
        describe.AddCommandPattern(prefix + "search <q>", "searches categories");
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
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

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
                else if (nextArg != null)
                {
                    isSearching = true;
                    this.Search = nextArg;
                    i++;
                }
                else
                {
                    Console.WriteLine("Search command requires a search expression. ");
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

        ok:
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
            Console.WriteLine("No current profile. ");
            return;
        }

        var store = profile.Data?.ActivityCategories;
        var existingItems = store.Items.ToList();
        var newItems = new List<MynatimeProfileDataActivityCategory>();
        var deletedItems = new List<MynatimeProfileDataActivityCategory>();
        var changedItems = new List<MynatimeProfileDataActivityCategory>();

        // refresh from service
        var hasRefreshed = false;
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
                var searchResult = await SearchItems(allItems, this.Search, false);
                items = searchResult
                   .Select(x => x.Item)
                   .ToList();
            }
            
            foreach (var item in items)
            {
                Console.Write(item.ToString());

                if (newItems.Contains(item))
                {
                    Console.Write("\t\tNEW!");
                }

                if (changedItems.Contains(item))
                {
                    Console.Write("\t\tCHANGED!");
                }

                Console.WriteLine();
            }

            foreach (var item in deletedItems)
            {
                Console.Write(item.ToString());
                Console.Write("\t\tDELETED!");
                Console.WriteLine();
            }
        }

        // save store if changed
        if (hasRefreshed)
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

        Console.WriteLine("Refreshing categories... ");
        // first try to get the page
        var page = await this.manatimeWebClient.GetNewActivityItemPage();
        if (page.Succeed)
        {
            // yeah!
        }
        else if (page.Errors?.Any(x => x.Code == "LoggedOut") ?? false)
        {
            // session expired: renew
            Console.WriteLine("  Renewing session... ");

            if (profile.LoginUsername == null || profile.LoginPassword == null)
            {
                throw new InvalidOperationException("Missing authentication information. ");
            }

            var loginPage = await this.manatimeWebClient.PrepareEmailPasswordAuthenticate();
            if (loginPage.Succeed)
            {
                var loginResultPage = await this.manatimeWebClient.EmailPasswordAuthenticate(profile.LoginUsername, profile.LoginPassword);
                if (loginResultPage.Succeed)
                {
                    page = await this.manatimeWebClient.GetNewActivityItemPage();
                    if (page.Succeed)
                    {
                        // yeah!
                    }
                    else
                    {
                        Console.WriteLine("Auto log-in failed: something went wrong 3. ");
                    }
                }
                else
                {
                    Console.WriteLine("Auto log-in failed: something went wrong 2. ");
                }
            }
            else
            {
                Console.WriteLine("Auto log-in failed: something went wrong 1. ");
            }
        }
        else
        {
            Console.WriteLine("Auto log-in failed: something went wrong 0. ");
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
                    category.UpdateFrom(match, page.LoadTime.Value);
                    changedItems.Add(match);
                }
                else
                {
                    match = new MynatimeProfileDataActivityCategory(category.Id, category.DisplayName);
                    match.Created = page.LoadTime;
                    category.UpdateFrom(match, page.LoadTime.Value);
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
            Console.WriteLine(page.ToString());
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
                levensteins[item] = Levenshtein.Distance(search.ToUpperInvariant(), item.Item.Name.ToUpperInvariant());
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
