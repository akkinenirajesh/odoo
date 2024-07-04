C#
public partial class MailPerformanceThread
{
    public void ValuePcCompute()
    {
        this.ValuePc = (float)this.Value / 100;
    }
}

public partial class MailTestFieldType
{
    public MailTestFieldType Create(Dictionary<string, object> vals)
    {
        if (!Env.Context.ContainsKey("default_type"))
        {
            Env.Context["default_type"] = "first";
        }
        return Env.Create<MailTestFieldType>(vals);
    }

    public List<string> MailGetPartnerFields(bool introspectFields)
    {
        return new List<string>() { "CustomerId" };
    }
}

public partial class MailTestLang
{
    public List<string> MailGetPartnerFields(bool introspectFields)
    {
        return new List<string>() { "CustomerId" };
    }

    public List<Tuple<string, Dictionary<string, object>>> NotifyGetRecipientsGroups(MailMessage message, string modelDescription, Dictionary<string, object> msgVals)
    {
        var groups = base.NotifyGetRecipientsGroups(message, modelDescription, msgVals);

        foreach (var group in groups.Where(g => g.Item1 == "follower" || g.Item1 == "customer"))
        {
            group.Item2["has_button_access"] = true;
            group.Item2["actions"] = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "url", this.NotifyGetActionLink("controller", "/test_mail/do_stuff", msgVals) },
                    { "title", Env.Translate("NotificationButtonTitle") }
                }
            };
        }

        return groups;
    }
}

public partial class MailTestTrackAll
{
    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("Many2ManyField"))
        {
            // TODO: Implement handling of Many2ManyField updates
        }
        if (vals.ContainsKey("One2ManyField"))
        {
            // TODO: Implement handling of One2ManyField updates
        }
        base.Write(vals);
    }
}

public partial class MailTestTrackCompute
{
    public MailTestTrackCompute Create(Dictionary<string, object> vals)
    {
        return Env.Create<MailTestTrackCompute>(vals);
    }
}

public partial class MailTestTrackSelection
{
    public MailTestTrackSelection Create(Dictionary<string, object> vals)
    {
        return Env.Create<MailTestTrackSelection>(vals);
    }
}

public partial class MailTestMultiCompany
{
    public MailTestMultiCompany Create(Dictionary<string, object> vals)
    {
        return Env.Create<MailTestMultiCompany>(vals);
    }
}

public partial class MailTestMultiCompanyRead
{
    public MailTestMultiCompanyRead Create(Dictionary<string, object> vals)
    {
        return Env.Create<MailTestMultiCompanyRead>(vals);
    }
}

public partial class MailTestMultiCompanyWithActivity
{
    public MailTestMultiCompanyWithActivity Create(Dictionary<string, object> vals)
    {
        return Env.Create<MailTestMultiCompanyWithActivity>(vals);
    }
}
