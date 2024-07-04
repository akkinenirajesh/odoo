csharp
public partial class MassMailing.LinkTrackerClick {

    public MassMailing.LinkTrackerClick PrepareClickValuesFromRoute(params object[] routeValues) {
        var clickValues = this.Env.Call("super", "_prepare_click_values_from_route", routeValues);
        if (clickValues.ContainsKey("MailingTraceId")) {
            var traceSudo = this.Env.Ref("mailing.trace").Sudo().Browse(clickValues["MailingTraceId"]);
            if (!traceSudo.Exists()) {
                clickValues["MailingTraceId"] = false;
            } else {
                if (!clickValues.ContainsKey("CampaignId")) {
                    clickValues["CampaignId"] = traceSudo.CampaignId.Id;
                }
                if (!clickValues.ContainsKey("MassMailingId")) {
                    clickValues["MassMailingId"] = traceSudo.MassMailingId.Id;
                }
            }
        }
        return clickValues;
    }

    public MassMailing.LinkTrackerClick AddClick(string code, params object[] routeValues) {
        var click = this.Env.Call("super", "add_click", code, routeValues);
        if (click != null && click.MailingTraceId != null) {
            click.MailingTraceId.SetOpened();
            click.MailingTraceId.SetClicked();
        }
        return click;
    }
}
