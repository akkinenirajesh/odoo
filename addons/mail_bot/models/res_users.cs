C#
public partial class Users {
    public virtual void InitMessaging(object store) {
        bool odoobotOnboarding = false;
        if (this.OdoobotState == null || this.OdoobotState.Equals("NotInitialized") && this.IsInternal()) {
            odoobotOnboarding = true;
            this.InitOdoobot();
        }
        var super = Env.GetModel("Users").Get("Users").InitMessaging(store);
        ((dynamic)store).Add(new { odoobotOnboarding = odoobotOnboarding });
    }

    public virtual void InitOdoobot() {
        var odoobotId = Env.GetModel("Ir.Model.Data").XmlidToResID("base.partner_root");
        var channel = Env.GetModel("Discuss.Channel").ChannelGet(new object[] { odoobotId, this.PartnerId.Id });
        var message = $"%s<br/>%s<br/><b>%s</b> <span class=\"o_odoobot_command\">:)</span>";
        channel.MessagePost(message, odoobotId, "comment", "mail.mt_comment");
        this.OdoobotState = "OnboardingEmoji";
    }

    public virtual bool IsInternal() {
        return true;
    }
}
