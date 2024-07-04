csharp
public partial class WebsiteVisitor {
    public bool CheckForSmsComposer() {
        return Env.Is(this.PartnerId) && (Env.Get<string>(this.PartnerId, "Mobile") != null || Env.Get<string>(this.PartnerId, "Phone") != null);
    }

    public Dictionary<string, object> PrepareSmsComposerContext() {
        return new Dictionary<string, object> {
            { "DefaultResModel", "Res.Partner" },
            { "DefaultResId", this.PartnerId },
            { "DefaultCompositionMode", "Comment" },
            { "DefaultNumberFieldName", Env.Get<string>(this.PartnerId, "Mobile") != null ? "Mobile" : "Phone" }
        };
    }

    public Dictionary<string, object> ActionSendSms() {
        if (!CheckForSmsComposer()) {
            throw new Exception("There are no contact and/or no phone or mobile numbers linked to this visitor.");
        }
        Dictionary<string, object> visitorComposerCtx = PrepareSmsComposerContext();

        Dictionary<string, object> composeCtx = Env.Context.ToDictionary();
        composeCtx.Update(visitorComposerCtx);
        return new Dictionary<string, object> {
            { "Name", "Send SMS Text Message" },
            { "Type", "ir.actions.act_window" },
            { "ResModel", "Sms.Composer" },
            { "ViewMode", "form" },
            { "Context", composeCtx },
            { "Target", "new" }
        };
    }
}
