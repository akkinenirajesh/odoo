csharp
public partial class WebsiteEventBooth.Event {
    public void ComputeBoothMenu() {
        if (this.EventType.BoothMenu == true && this.EventType != this.OriginalEventType) {
            this.BoothMenu = true;
        } else if (this.WebsiteMenu == true && (this.WebsiteMenu != this.OriginalWebsiteMenu || this.BoothMenu == false)) {
            this.BoothMenu = true;
        } else if (this.WebsiteMenu == false) {
            this.BoothMenu = false;
        }
    }
    public void ToggleBoothMenu(bool val) {
        this.BoothMenu = val;
    }
    public List<string> GetMenuUpdateFields() {
        var fields = base.GetMenuUpdateFields();
        fields.Add("BoothMenu");
        return fields;
    }
    public void UpdateWebsiteMenus(Dictionary<string, List<WebsiteEventBooth.Event>> menusUpdateByField = null) {
        base.UpdateWebsiteMenus(menusUpdateByField);
        if (this.Menu.HasValue && (menusUpdateByField == null || menusUpdateByField.ContainsKey("BoothMenu") && menusUpdateByField["BoothMenu"].Contains(this))) {
            this.UpdateWebsiteMenuEntry("BoothMenu", "BoothMenuIds", "booth");
        }
    }
    public Dictionary<string, string> GetMenuTypeFieldMatching() {
        var res = base.GetMenuTypeFieldMatching();
        res.Add("booth", "BoothMenu");
        return res;
    }
    public List<Tuple<string, string, bool, int, string>> GetWebsiteMenuEntries() {
        var entries = base.GetWebsiteMenuEntries();
        entries.Add(new Tuple<string, string, bool, int, string>("Get A Booth", string.Format("/event/{0}/booth", Env.Slug(this)), false, 90, "booth"));
        return entries;
    }
}
