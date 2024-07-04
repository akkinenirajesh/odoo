csharp
public partial class MailTemplate
{
    public IEnumerable<MailTemplate> NameSearch(string name, IEnumerable<object> domain = null, string @operator = "ilike", int? limit = null, string order = null)
    {
        if (Env.Context.ContainsKey("filter_template_on_event") && (bool)Env.Context["filter_template_on_event"])
        {
            domain = Expression.And(new[] { ("Model", "=", "Event.Registration") }, domain ?? new object[] { });
        }
        // Assuming a base implementation of NameSearch exists in a base class or interface
        return base.NameSearch(name, domain, @operator, limit, order);
    }

    public override bool Unlink()
    {
        bool result = base.Unlink();

        string domain = ("TemplateRef", "in", new[] { $"{this.GetType().Name},{this.Id}" });
        Env.Get<EventMail>().Sudo().Search(new[] { domain }).Unlink();
        Env.Get<EventTypeMail>().Sudo().Search(new[] { domain }).Unlink();

        return result;
    }
}
