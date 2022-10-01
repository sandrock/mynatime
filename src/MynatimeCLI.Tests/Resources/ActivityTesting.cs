
namespace Mynatime.CLI.Tests.Resources;

using Mynatime.Infrastructure;
using System;

public class ActivityTesting
{
    public static void PopulateCategories0(MynatimeProfileDataActivityCategories list)
    {
        var id = 1000;
        list.Add(new MynatimeProfileDataActivityCategory("1001", "proj1"));
    }

    public static void PopulateCategories1(MynatimeProfileDataActivityCategories list)
    {
        var id = 1000;
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "MyCompany-Interne"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Formation Interne"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "DSI-Maintenances"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "DSI-Missions"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "DSI-Interventions"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Prospects"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project ASDFG"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project TYUI1"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project TYUI2"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project TYUI3"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project TYUI3-Branch2"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project CVBNM"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project ASDFGHJ"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project TYUIOP"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Project GHJKL"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Supertron-Maintenance"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Supertron-Formation"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "Salons et interventions externes"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "R&D-YUIOP"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "R&D-HJKL2"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "R&D-RTYUI"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "R&D-ZXCVB"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "R&D-QWERT"));
        list.Add(new MynatimeProfileDataActivityCategory((++id).ToInvariantString(), "R&D-Interne"));
    }
}
