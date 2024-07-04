csharp
public partial class WebsiteMenu 
{
    public WebsiteMenu Unlink() 
    {
        Dictionary<WebsiteEvent, List<string>> eventUpdates = new Dictionary<WebsiteEvent, List<string>>();
        var websiteEventMenus = Env.Search<WebsiteEventMenu>(new List<object[]> { new object[] { "MenuID", "in", this.Id } });

        foreach (var eventMenu in websiteEventMenus)
        {
            if (!eventUpdates.ContainsKey(eventMenu.EventID))
            {
                eventUpdates.Add(eventMenu.EventID, new List<string>());
            }

            if (eventMenu.MenuType == "track" && eventMenu.MenuID.Url.Contains("/track"))
            {
                eventUpdates[eventMenu.EventID].Add("WebsiteTrack");
            }
        }

        // Call super to unlink menu entries
        var res = base.Unlink();

        // Update events
        foreach (var eventToUpdate in eventUpdates)
        {
            if (eventToUpdate.Value.Count > 0)
            {
                eventToUpdate.Key.Write(eventToUpdate.Value.Select(fname => new KeyValuePair<string, object>(fname, false)).ToDictionary(x => x.Key, x => x.Value));
            }
        }

        return res;
    }
}
