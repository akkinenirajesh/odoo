csharp
public partial class MailIrUiMenu {

    public int GetBestBackendRootMenuIdForModel(string resModel) {
        // if no access to the menu, return None
        try {
            var visibleMenuIds = Env.Ref("Mail.IrUiMenu")._VisibleMenuIds();
            // Try first to get a menu root from the model implementation (take the less specialized i.e. the first one)
            var menuRootCandidates = Env.Ref(resModel)._GetBackendRootMenuIds();
            var menuRootId = menuRootCandidates.FirstOrDefault(m_id => visibleMenuIds.Contains(m_id));
            if (menuRootId != 0) {
                return menuRootId;
            }

            // No menu root could be found by interrogating the model so fall back to a simple heuristic
            // Prefetch menu fields and all menu's actions of type act_window
            var menus = Env.Ref("Mail.IrUiMenu").Browse(visibleMenuIds);
            var actions = Env.Ref("Ir.Actions.ActWindow").Sudo().Browse(menus.Select(menu => int.Parse(menu["Action"].Split(',')[1])).ToList()).Where(action => !string.IsNullOrEmpty(action.ResModel)).ToList();

            var menuSudo = actions.OrderByDescending(action => !string.IsNullOrEmpty(action.Path))
                .ThenByDescending(action => action.Id)
                .Where(action => action.Type == "ir.actions.act_window" && action.ResModel == resModel
                    && menus.Any(menu => menu.ParentPath.Split('/').All(menuId => visibleMenuIds.Contains(int.Parse(menuId)))))
                .Select(action => menus.FirstOrDefault(menu => menu.Action == $"ir.actions.act_window,{action.Id}"))
                .FirstOrDefault();

            return menuSudo != null ? int.Parse(menuSudo.ParentPath.Split('/')[0]) : 0;
        }
        catch (AccessError) {
            return 0;
        }
    }
}
