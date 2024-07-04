csharp
public partial class SlideChannel {
    public virtual void ActionMassMailingAttendees() {
        string domain = $"[('SlideChannelIds', 'in', this.Ids)]";
        var massMailingAction = new Dictionary<string, object>()
        {
            { "Name", Env.Translate("Mass Mail Course Members") },
            { "Type", "ir.actions.act_window" },
            { "ResModel", "Mailing.Mailing" },
            { "ViewMode", "form" },
            { "Target", "current" },
            { "Context", new Dictionary<string, object>()
                {
                    { "DefaultMailingModelId", Env.Ref("base.model_res_partner").Id },
                    { "DefaultMailingDomain", domain }
                }
            }
        };
        return massMailingAction;
    }
}
