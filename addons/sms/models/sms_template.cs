csharp
public partial class SmsSMSTemplate
{
    public virtual void ComputeRenderModel()
    {
        this.RenderModel = this.Model;
    }

    public virtual List<object> CopyData(object defaultValues)
    {
        List<object> valsList = base.CopyData(defaultValues);
        return valsList.Select((vals, index) => new {
            Name = string.Format("{0} (copy)", this.Name),
            vals
        }).ToList();
    }

    public virtual void Unlink()
    {
        Env.Ref<Ir.Actions.ActWindow>("sms.sms_composer_view_form");
        Env.Model("sms.composer");
        Env.Model("ir.actions.act_window");
        this.SidebarActionId.Unlink();
        base.Unlink();
    }

    public virtual void ActionCreateSidebarAction()
    {
        var ActWindow = Env.Model<Ir.Actions.ActWindow>();
        var view = Env.Ref<Ir.Actions.ActWindow>("sms.sms_composer_view_form");

        foreach (var template in this)
        {
            var buttonName = string.Format("Send SMS ({0})", template.Name);
            var action = ActWindow.Create(new {
                Name = buttonName,
                Type = "ir.actions.act_window",
                ResModel = "sms.composer",
                // Add default_composition_mode to guess to determine if need to use mass or comment composer
                Context = string.Format("{{'default_template_id' : {0}, 'sms_composition_mode': 'guess', 'default_res_ids': active_ids, 'default_res_id': active_id}}", template.Id),
                ViewMode = "form",
                ViewId = view.Id,
                Target = "new",
                BindingModelId = template.ModelId.Id,
            });
            template.SidebarActionId = action;
        }
    }

    public virtual void ActionUnlinkSidebarAction()
    {
        foreach (var template in this)
        {
            if (template.SidebarActionId != null)
            {
                template.SidebarActionId.Unlink();
            }
        }
    }
}
