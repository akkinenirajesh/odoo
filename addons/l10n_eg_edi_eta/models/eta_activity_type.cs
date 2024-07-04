csharp
public partial class ActivityType
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<ActivityType> NameSearch(string name, IEnumerable<object> domain = null, string @operator = "ilike", int? limit = null, string order = null)
    {
        if (domain == null)
        {
            domain = new List<object>();
        }

        if (@operator != "ilike" || string.IsNullOrWhiteSpace(name))
        {
            var newDomain = new List<object>
            {
                "|",
                new[] { "Name", @operator, name },
                new[] { "Code", @operator, name }
            };
            domain = newDomain.Concat(domain);
        }

        return Env.Search<ActivityType>(domain, limit: limit, order: order);
    }
}
