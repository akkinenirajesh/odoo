csharp
public partial class Users
{
    public string MfaType()
    {
        var superResult = base.MfaType();
        if (superResult != null)
        {
            return superResult;
        }

        var ICP = Env.Get<IrConfigParameter>().Sudo();
        var otpRequired = false;
        if (ICP.GetParam("auth_totp.policy") == "all_required")
        {
            otpRequired = true;
        }
        else if (ICP.GetParam("auth_totp.policy") == "employee_required" && this.IsInternal())
        {
            otpRequired = true;
        }
        
        return otpRequired ? "totp_mail" : null;
    }

    public string MfaUrl()
    {
        var superResult = base.MfaUrl();
        if (superResult != null)
        {
            return superResult;
        }

        return this.MfaType() == "totp_mail" ? "/web/login/totp" : null;
    }

    public bool TotpCheck(string code)
    {
        this.TotpRateLimit("code_check");
        var user = this.Sudo();
        if (user.MfaType() != "totp_mail")
        {
            return base.TotpCheck(code);
        }

        var key = user.GetTotpMailKey();
        var match = TOTP.Match(key, code, window: 3600, timestep: 3600);
        if (match == null)
        {
            Logger.Info($"2FA check (mail): FAIL for {user} {user.Login}");
            throw new AccessDeniedException("Verification failed, please double-check the 6-digit code");
        }
        
        Logger.Info($"2FA check(mail): SUCCESS for {user} {user.Login}");
        this.TotpRateLimitPurge("code_check");
        this.TotpRateLimitPurge("send_email");
        return true;
    }

    public byte[] GetTotpMailKey()
    {
        return Env.Hmac("auth_totp_mail-code", (this.Id, this.Login, this.LoginDate)).GetBytes();
    }

    public (string, string) GetTotpMailCode()
    {
        var key = this.GetTotpMailKey();
        var now = DateTime.Now;
        var counter = (int)(now.Timestamp() / 3600);

        var code = HOTP.Generate(key, counter);
        var expiration = TimeSpan.FromSeconds(3600);
        var lang = BabelLocale.Parse(Env.Context.GetValueOrDefault("lang", this.Lang));
        var expirationFormatted = Babel.FormatTimeDelta(expiration, locale: lang);

        return (code.ToString().PadLeft(6, '0'), expirationFormatted);
    }

    public void SendTotpMailCode()
    {
        this.TotpRateLimit("send_email");

        if (string.IsNullOrEmpty(this.Email))
        {
            throw new UserException($"Cannot send email: user {this.Name} has no email address.");
        }

        var template = Env.Ref<MailTemplate>("auth_totp_mail_enforce.mail_template_totp_mail_code").Sudo();
        var context = new Dictionary<string, object>();
        
        if (HttpContext.Current != null)
        {
            var request = HttpContext.Current.Request;
            var device = request.UserAgent.Platform;
            var browser = request.UserAgent.Browser;
            context["location"] = null;
            context["device"] = device != null ? char.ToUpper(device[0]) + device.Substring(1) : null;
            context["browser"] = browser != null ? char.ToUpper(browser[0]) + browser.Substring(1) : null;
            context["ip"] = request.UserHostAddress;

            if (!string.IsNullOrEmpty(request.GeoIp.City.Name))
            {
                context["location"] = $"{request.GeoIp.City.Name}, {request.GeoIp.CountryName}";
            }
        }

        var emailValues = new Dictionary<string, object>
        {
            ["email_to"] = this.Email,
            ["email_cc"] = false,
            ["auto_delete"] = true,
            ["recipient_ids"] = new List<int>(),
            ["partner_ids"] = new List<int>(),
            ["scheduled_date"] = false
        };

        using (var transaction = Env.BeginTransaction())
        {
            template.WithContext(context).SendMail(
                this.Id, 
                forceCreate: true, 
                raiseException: true, 
                emailValues: emailValues, 
                emailLayoutXmlid: "mail.mail_notification_light"
            );
            transaction.Commit();
        }
    }

    private void TotpRateLimit(string limitType)
    {
        if (HttpContext.Current == null)
        {
            throw new Exception("A request is required to be able to rate limit TOTP related actions");
        }

        var (limit, interval) = TOTP_RATE_LIMITS[limitType];
        var rateLimitLog = Env.Get<AuthTotpRateLimitLog>().Sudo();
        var ip = HttpContext.Current.Request.UserHostAddress;
        var domain = new List<object>
        {
            ("user_id", "=", this.Id),
            ("create_date", ">=", DateTime.Now.AddSeconds(-interval)),
            ("limit_type", "=", limitType),
            ("ip", "=", ip)
        };

        var count = rateLimitLog.SearchCount(domain);
        if (count >= limit)
        {
            var descriptions = new Dictionary<string, string>
            {
                ["send_email"] = "You reached the limit of authentication mails sent for your account",
                ["code_check"] = "You reached the limit of code verifications for your account"
            };
            var description = descriptions.GetValueOrDefault(limitType);
            throw new AccessDeniedException(description);
        }

        rateLimitLog.Create(new Dictionary<string, object>
        {
            ["user_id"] = this.Id,
            ["ip"] = ip,
            ["limit_type"] = limitType
        });
    }

    private void TotpRateLimitPurge(string limitType)
    {
        if (HttpContext.Current == null)
        {
            throw new Exception("A request is required to be able to rate limit TOTP related actions");
        }

        var ip = HttpContext.Current.Request.UserHostAddress;
        var rateLimitLog = Env.Get<AuthTotpRateLimitLog>().Sudo();
        rateLimitLog.Search(new List<object>
        {
            ("user_id", "=", this.Id),
            ("limit_type", "=", limitType),
            ("ip", "=", ip)
        }).Unlink();
    }
}
