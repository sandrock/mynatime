﻿namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

public class MynatimeProfileData : JsonObject
{
    private MynatimeProfileDataActivityCategories? activityCategories;

    public MynatimeProfileData()
        : base("Data", new JObject())
    {
    }

    internal MynatimeProfileData(JObject element)
        : base("Data", element)
    {
    }

    public MynatimeProfileDataActivityCategories? ActivityCategories
    {
        get
        {
            if (this.activityCategories != null)
            {
            }
            else if (this.Element.TryGetValue("ActivityCategories", out JToken? child))
            {
                this.activityCategories = new MynatimeProfileDataActivityCategories((JObject)child);
            }
            else if (!this.IsFrozen)
            {
                this.activityCategories = new MynatimeProfileDataActivityCategories(new JObject());
                this.Element.Add("ActivityCategories", this.activityCategories.Element);
            }

            return this.activityCategories;
        }
    }

    public MynatimeProfileDataActivityCategory? GetActivityById(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return this.ActivityCategories?.Items.FirstOrDefault(x => x.Id == id);
    }
}
