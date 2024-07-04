csharp
public partial class Website.IrUiMenu
{
    public Website.IrUiMenu LoadMenusRoot()
    {
        var rootMenus = Env.GetService("Website.IrUiMenu").Call("load_menus_root");
        if (Env.Context.ContainsKey("force_action"))
        {
            var webMenus = LoadWebMenus(Env.Session.ContainsKey("debug") && Env.Session.GetBool("debug"));
            foreach (var menu in rootMenus.Get("children").ToList())
            {
                if (
                    string.IsNullOrEmpty(menu.Get("action").ToString()) && 
                    webMenus.ContainsKey(menu.Get("id").ToString()) && 
                    !string.IsNullOrEmpty(webMenus[menu.Get("id").ToString()].Get("actionModel").ToString()) && 
                    !string.IsNullOrEmpty(webMenus[menu.Get("id").ToString()].Get("actionID").ToString())
                )
                {
                    menu.Set("action", $"{webMenus[menu.Get("id").ToString()].Get("actionModel")},{webMenus[menu.Get("id").ToString()].Get("actionID")}");
                }
            }
        }

        return rootMenus;
    }

    private object LoadWebMenus(bool debug)
    {
        return Env.GetService("Website.IrUiMenu").Call("load_web_menus", debug);
    }
}
