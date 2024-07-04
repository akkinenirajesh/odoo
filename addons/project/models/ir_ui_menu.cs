csharp
public partial class Project.IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        List<int> res = Env.Call("Project.IrUiMenu", "_load_menus_blacklist");
        if (!Env.User.IsInGroup("project.group_project_manager"))
        {
            res.Add(Env.Ref("project.rating_rating_menu_project").Id);
        }
        if (Env.User.IsInGroup("project.group_project_stages"))
        {
            res.Add(Env.Ref("project.menu_projects").Id);
            res.Add(Env.Ref("project.menu_projects_config").Id);
        }
        return res;
    }
}
