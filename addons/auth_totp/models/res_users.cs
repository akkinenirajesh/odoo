csharp
public partial class User
{
    public string[] SELF_READABLE_FIELDS => base.SELF_READABLE_FIELDS.Concat(new[] { "TotpEnabled", "TotpTrustedDeviceIds" }).ToArray();

    public string MfaType()
    {
        var baseType = base.MfaType();
        if (baseType != null)
            return baseType;
        
        return TotpEnabled ? "totp" : null;
    }

    public bool ShouldAlertNewDevice()
    {
        if (HttpContext.Current != null && MfaType() != null)
        {
            var key = HttpContext.Current.Request.Cookies["td_id"];
            if (!string.IsNullOrEmpty(key))
            {
                if (Env.Get<Auth.Device>().CheckCredentialsForUid(scope: "browser", key: key, uid: Id))
                {
                    return false;
                }
            }
            return true;
        }
        return base.ShouldAlertNewDevice();
    }

    public string MfaUrl()
    {
        var baseUrl = base.MfaUrl();
        if (baseUrl != null)
            return baseUrl;
        
        return MfaType() == "totp" ? "/web/login/totp" : null;
    }

    public void ComputeTotpEnabled()
    {
        TotpEnabled = !string.IsNullOrEmpty(TotpSecret);
    }

    public bool RpcApiKeysOnly()
    {
        return TotpEnabled || base.RpcApiKeysOnly();
    }

    public string[] GetSessionTokenFields()
    {
        return base.GetSessionTokenFields().Concat(new[] { "TotpSecret" }).ToArray();
    }

    public void TotpCheck(string code)
    {
        var key = Convert.FromBase64String(TotpSecret);
        var match = new TOTP(key).Match(code);
        if (match == null)
        {
            Logger.Info($"2FA check: FAIL for {this} {Login}");
            throw new AccessDeniedException("Verification failed, please double-check the 6-digit code");
        }
        Logger.Info($"2FA check: SUCCESS for {this} {Login}");
    }

    public bool TotpTrySetting(string secret, string code)
    {
        if (TotpEnabled || this != Env.User)
        {
            Logger.Info($"2FA enable: REJECT for {this} {Login}");
            return false;
        }

        secret = Regex.Replace(secret, @"\s", "").ToUpper();
        var match = new TOTP(Convert.FromBase64String(secret)).Match(code);
        if (match == null)
        {
            Logger.Info($"2FA enable: REJECT CODE for {this} {Login}");
            return false;
        }

        TotpSecret = secret;
        if (HttpContext.Current != null)
        {
            Env.FlushAll();
            var newToken = ComputeSessionToken(HttpContext.Current.Session.SessionID);
            HttpContext.Current.Session["session_token"] = newToken;
        }

        Logger.Info($"2FA enable: SUCCESS for {this} {Login}");
        return true;
    }

    [CheckIdentity]
    public Dictionary<string, object> ActionTotpDisable()
    {
        var logins = string.Join(", ", this.Select(u => u.Login));
        if (!(this == Env.User || Env.User.IsAdmin() || Env.Su))
        {
            Logger.Info($"2FA disable: REJECT for {this} ({logins}) by uid #{Env.User.Id}");
            return false;
        }

        RevokeAllDevices();
        TotpSecret = null;

        if (HttpContext.Current != null && this == Env.User)
        {
            Env.FlushAll();
            var newToken = ComputeSessionToken(HttpContext.Current.Session.SessionID);
            HttpContext.Current.Session["session_token"] = newToken;
        }

        Logger.Info($"2FA disable: SUCCESS for {this} ({logins}) by uid #{Env.User.Id}");
        return new Dictionary<string, object>
        {
            { "type", "ir.actions.client" },
            { "tag", "display_notification" },
            { "params", new Dictionary<string, object>
                {
                    { "type", "warning" },
                    { "message", $"Two-factor authentication disabled for the following user(s): {string.Join(", ", this.Select(u => u.Name))}" },
                    { "next", new Dictionary<string, string> { { "type", "ir.actions.act_window_close" } } }
                }
            }
        };
    }

    [CheckIdentity]
    public Dictionary<string, object> ActionTotpEnableWizard()
    {
        if (Env.User != this)
        {
            throw new UserException("Two-factor authentication can only be enabled for yourself");
        }

        if (TotpEnabled)
        {
            throw new UserException("Two-factor authentication already enabled");
        }

        var secretBytesCount = TOTP_SECRET_SIZE / 8;
        var secret = string.Join(" ", Regex.Matches(Convert.ToBase64String(RandomNumberGenerator.GetBytes(secretBytesCount)), ".{1,4}"));

        var wizard = Env.Get<Auth.TotpWizard>().Create(new Dictionary<string, object>
        {
            { "UserId", Id },
            { "Secret", secret }
        });

        return new Dictionary<string, object>
        {
            { "type", "ir.actions.act_window" },
            { "target", "new" },
            { "res_model", "auth_totp.wizard" },
            { "name", "Two-Factor Authentication Activation" },
            { "res_id", wizard.Id },
            { "views", new List<object> { new List<object> { false, "form" } } },
            { "context", Env.Context }
        };
    }

    [CheckIdentity]
    public void RevokeAllDevices()
    {
        _RevokeAllDevices();
    }

    private void _RevokeAllDevices()
    {
        TotpTrustedDeviceIds.Remove();
    }

    public void ChangePassword(string oldPasswd, string newPasswd)
    {
        _RevokeAllDevices();
        base.ChangePassword(oldPasswd, newPasswd);
    }

    private void _ComputeTotpSecret()
    {
        var result = Env.Cr.Query("SELECT totp_secret FROM res_users WHERE id=@Id", new { Id });
        TotpSecret = result.FirstOrDefault()?.totp_secret;
    }

    private void _InverseToken()
    {
        Env.Cr.Execute("UPDATE res_users SET totp_secret = @Secret WHERE id=@Id", new { Secret = TotpSecret ?? null, Id });
    }

    private List<int> _TotpEnableSearch(string @operator, bool value)
    {
        value = @operator == "!=" ? !value : value;
        var query = value
            ? "SELECT id FROM res_users WHERE totp_secret IS NOT NULL"
            : "SELECT id FROM res_users WHERE totp_secret IS NULL OR totp_secret='false'";
        
        var result = Env.Cr.Query<int>(query);
        return result.ToList();
    }
}
