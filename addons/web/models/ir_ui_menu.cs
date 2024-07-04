csharp
public partial class WebIrUiMenu {
    // all the model methods are written here.
    public List<Dictionary<string, object>> LoadWebMenus(bool debug) {
        // Loads all menu items (all applications and their sub-menus) and
        // processes them to be used by the webclient. Mainly, it associates with
        // each application (top level menu) the action of its first child menu
        // that is associated with an action (recursively), i.e. with the action
        // to execute when the opening the app.
        //
        // :return: the menus (including the images in Base64)
        List<Dictionary<string, object>> menus = LoadMenus(debug);
        Dictionary<string, object> webMenus = new Dictionary<string, object>();
        foreach (Dictionary<string, object> menu in menus.Values) {
            if (!(bool)menu["Id"]) {
                // special root menu case
                webMenus["root"] = new Dictionary<string, object> {
                    { "Id", "root" },
                    { "Name", menu["Name"] },
                    { "Children", menu["Children"] },
                    { "AppID", false },
                    { "Xmlid", "" },
                    { "ActionID", false },
                    { "ActionModel", false },
                    { "ActionPath", false },
                    { "WebIcon", null },
                    { "WebIconData", null },
                    { "WebIconDataMimetype", null },
                    { "BackgroundImage", menu.ContainsKey("BackgroundImage") ? menu["BackgroundImage"] : null }
                };
            } else {
                string action = (string)menu["Action"];
                if ((int)menu["Id"] == (int)menu["AppID"]) {
                    // if it's an app take action of first (sub)child having one defined
                    Dictionary<string, object> child = menu;
                    while (child != null && action == null) {
                        action = (string)child["Action"];
                        child = menus[child["Children"][0]] as Dictionary<string, object>;
                    }
                }
                string[] actionParts = action.Split(',');
                string actionModel = actionParts[0];
                int actionId = actionParts.Length > 1 ? Convert.ToInt32(actionParts[1]) : 0;
                string actionPath = actionModel != null && actionId > 0 ? Env.GetModel(actionModel).Browse(actionId).Path : null;
                webMenus[(int)menu["Id"]] = new Dictionary<string, object> {
                    { "Id", menu["Id"] },
                    { "Name", menu["Name"] },
                    { "Children", menu["Children"] },
                    { "AppID", menu["AppID"] },
                    { "Xmlid", menu["Xmlid"] },
                    { "ActionID", actionId },
                    { "ActionModel", actionModel },
                    { "ActionPath", actionPath },
                    { "WebIcon", menu["WebIcon"] },
                    { "WebIconData", menu["WebIconData"] },
                    { "WebIconDataMimetype", menu["WebIconDataMimetype"] }
                };
            }
        }
        return webMenus;
    }
    public List<Dictionary<string, object>> LoadMenus(bool debug) {
        // TODO: implement LoadMenus method
        return new List<Dictionary<string, object>>();
    }
}
