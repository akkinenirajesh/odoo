csharp
public partial class MailIrMailServer
{
    public virtual void ActiveUsagesCompute()
    {
        var usagesSuper = Env.CallMethod("super", "_active_usages_compute");
        if (this.MailTemplateIds.Any())
        {
            foreach (var template in this.MailTemplateIds)
            {
                usagesSuper.Add(string.Format("{0} (Email Template)", template.DisplayName));
            }
        }
    }

    public virtual string GetDefaultBounceAddress()
    {
        if (!string.IsNullOrEmpty(Env.Company.BounceEmail))
        {
            return Env.Company.BounceEmail;
        }
        return Env.CallMethod("super", "_get_default_bounce_address");
    }

    public virtual string GetDefaultFromAddress()
    {
        if (!string.IsNullOrEmpty(Env.Company.DefaultFromEmail))
        {
            return Env.Company.DefaultFromEmail;
        }
        return Env.CallMethod("super", "_get_default_from_address");
    }

    public virtual string GetTestEmailFrom()
    {
        if (!string.IsNullOrEmpty(this.FromFilter))
        {
            var fromFilterParts = this.FromFilter.Split(',').Select(x => x.Trim()).ToList();
            if (fromFilterParts.Any(x => x.Contains('@')))
            {
                return fromFilterParts.FirstOrDefault(x => x.Contains('@'));
            }
            var aliasDomains = Env.Model("Mail.MailAliasDomain").Search([]);
            var matching = aliasDomains.FirstOrDefault(x => MatchFromFilter(x.DefaultFromEmail, this.FromFilter));
            if (matching != null)
            {
                return matching.DefaultFromEmail;
            }
            return $"odoo@{fromFilterParts[0]}";
        }
        return Env.CallMethod("super", "_get_test_email_from");
    }

    public virtual bool MatchFromFilter(string email, string fromFilter)
    {
        if (string.IsNullOrEmpty(fromFilter))
        {
            return true;
        }

        var fromFilterParts = fromFilter.Split(',').Select(x => x.Trim()).ToList();
        return fromFilterParts.Any(x => 
        {
            if (x == "*")
            {
                return true;
            }
            else if (x.Contains("@"))
            {
                return email == x;
            }
            else
            {
                return email.EndsWith("@" + x);
            }
        });
    }
}
