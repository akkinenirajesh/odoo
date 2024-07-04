csharp
public partial class SmsTemplate
{
    public IEnumerable<SmsTemplate> NameSearch(string name, IEnumerable<object> domain = null, string @operator = "ilike", int? limit = null, string order = null)
    {
        if (Env.Context.ContainsKey("filter_template_on_event") && (bool)Env.Context["filter_template_on_event"])
        {
            domain = Expression.AND(new[] { new[] { ("Model", "=", "Event.Registration") }, domain ?? new object[] { } });
        }
        return base.NameSearch(name, domain, @operator, limit, order);
    }

    public override bool Unlink()
    {
        bool res = base.Unlink();

        var templateRefs = this.Select(template => $"{template.GetType().Name},{template.Id}");
        var domain = ("TemplateRef", "in", templateRefs);

        Env.Sudo().Get<EventMail>().Search(new[] { domain }).Unlink();
        Env.Sudo().Get<EventTypeMail>().Search(new[] { domain }).Unlink();

        return res;
    }

    public override string ToString()
    {
        return Name;
    }
}
