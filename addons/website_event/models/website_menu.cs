csharp
public partial class WebsiteMenu {
    public WebsiteMenu Unlink() {
        var eventUpdates = new Dictionary<WebsiteEvent, List<string>>();
        var websiteEventMenus = Env.GetRecordCollection<WebsiteEventMenu>().Search(x => x.MenuID == this.ID);
        foreach (var eventMenu in websiteEventMenus) {
            var toUpdate = eventUpdates.GetOrAdd(eventMenu.EventID, new List<string>());
            foreach (var menuType in eventMenu.EventID.GetMenuTypeFieldMatching()) {
                if (eventMenu.MenuType == menuType.Key) {
                    toUpdate.Add(menuType.Value);
                }
            }
        }

        // manually remove website_event_menus to call their ``unlink`` method. Otherwise
        // super unlinks at db level and skip model-specific behavior.
        websiteEventMenus.Unlink();
        var res = base.Unlink();

        // update events
        foreach (var eventToUpdate in eventUpdates) {
            if (eventToUpdate.Value.Any()) {
                eventToUpdate.Key.Write(eventToUpdate.Value.Select(x => new KeyValuePair<string, object>(x, false)).ToDictionary(x => x.Key, x => x.Value));
            }
        }

        return res;
    }
}
