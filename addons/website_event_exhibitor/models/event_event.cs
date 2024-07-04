csharp
public partial class WebsiteEventExhibitor.EventEvent
{
    public void ComputeSponsorCount()
    {
        var data = Env.Get("event.sponsor")._ReadGroup(new List<object>() { new object[] { "event_id", "in", this.Id } }, new List<object>() { "event_id" }, new List<object>() { "__count" });
        var result = new Dictionary<long, int>();
        foreach (var item in data)
        {
            result.Add(Convert.ToInt64(item["event_id"]), Convert.ToInt32(item["__count"]));
        }
        SponsorCount = result.ContainsKey(this.Id) ? result[this.Id] : 0;
    }

    public void ComputeExhibitorMenu()
    {
        if (EventTypeId != null && EventTypeId != EventTypeIdOriginal)
        {
            ExhibitorMenu = EventTypeId.ExhibitorMenu;
        }
        else if (WebsiteMenu && (WebsiteMenu != WebsiteMenuOriginal || !ExhibitorMenu))
        {
            ExhibitorMenu = true;
        }
        else if (!WebsiteMenu)
        {
            ExhibitorMenu = false;
        }
    }

    public void ToggleExhibitorMenu(bool val)
    {
        ExhibitorMenu = val;
    }

    public List<object> GetMenuUpdateFields()
    {
        var res = base.GetMenuUpdateFields();
        res.Add("ExhibitorMenu");
        return res;
    }

    public void UpdateWebsiteMenus(Dictionary<string, List<object>> menusUpdateByField = null)
    {
        base.UpdateWebsiteMenus(menusUpdateByField);
        if (MenuId != null && (menusUpdateByField == null || menusUpdateByField.ContainsKey("ExhibitorMenu") && menusUpdateByField["ExhibitorMenu"].Contains(this)))
        {
            _UpdateWebsiteMenuEntry("ExhibitorMenu", "ExhibitorMenuIds", "exhibitor");
        }
    }

    public Dictionary<string, string> GetMenuTypeFieldMatching()
    {
        var res = base.GetMenuTypeFieldMatching();
        res["exhibitor"] = "ExhibitorMenu";
        return res;
    }

    public List<object> GetWebsiteMenuEntries()
    {
        var res = base.GetWebsiteMenuEntries();
        res.Add(new object[] { "Exhibitors", $"/event/{Slug(this)}/exhibitors", false, 60, "exhibitor" });
        return res;
    }
}
