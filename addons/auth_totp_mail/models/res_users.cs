csharp
public partial class ResUsers
{
    public override string Write(Dictionary<string, object> vals)
    {
        var res = base.Write(vals);

        if (vals.ContainsKey("TotpSecret"))
        {
            if (vals["TotpSecret"] != null)
            {
                NotifySecuritySettingUpdate(
                    "Security Update: 2FA Activated",
                    "Two-factor authentication has been activated on your account",
                    suggestTwoFa: false
                );
            }
            else
            {
                NotifySecuritySettingUpdate(
                    "Security Update: 2FA Deactivated",
                    "Two-factor authentication has been deactivated on your account",
                    suggestTwoFa: false
                );
            }
        }

        return res;
    }

    public Dictionary<string, object> NotifySecuritySettingUpdatePrepareValues(string content, bool suggestTwoFa = true, Dictionary<string, object> kwargs = null)
    {
        var values = base.NotifySecuritySettingUpdatePrepareValues(content, kwargs);
        values["suggest_2fa"] = suggestTwoFa && !this.TotpEnabled;
        return values;
    }

    public Dictionary<string, object> ActionOpenMyAccountSettings()
    {
        return new Dictionary<string, object>
        {
            ["name"] = "Account Security",
            ["type"] = "ir.actions.act_window",
            ["res_model"] = "Auth.ResUsers",
            ["views"] = new List<object> { new List<object> { Env.Ref("auth_totp_mail.res_users_view_form").Id, "form" } },
            ["res_id"] = this.Id
        };
    }

    public string GetTotpInviteUrl()
    {
        return "/web#action=auth_totp_mail.action_activate_two_factor_authentication";
    }

    public Dictionary<string, object> ActionTotpInvite()
    {
        var inviteTemplate = Env.Ref("auth_totp_mail.mail_template_totp_invite");
        var usersToInvite = Env.ResUsers.Search(u => u.TotpSecret == null);

        foreach (var user in usersToInvite)
        {
            var emailValues = new Dictionary<string, object>
            {
                ["email_from"] = Env.User.EmailFormatted,
                ["author_id"] = Env.User.PartnerId
            };

            inviteTemplate.SendMail(user.Id, forceSend: true, emailValues: emailValues, emailLayoutXmlid: "mail.mail_notification_light");
        }

        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.client",
            ["tag"] = "display_notification",
            ["params"] = new Dictionary<string, object>
            {
                ["type"] = "info",
                ["sticky"] = false,
                ["message"] = $"Invitation to use two-factor authentication sent for the following user(s): {string.Join(", ", usersToInvite.Select(u => u.Name))}"
            }
        };
    }
}
