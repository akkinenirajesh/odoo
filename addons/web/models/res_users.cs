csharp
public partial class WebResUsers
{
    public virtual List<object> NameSearch(string name, List<object> domain = null, string @operator = "ilike", int? limit = null, string order = null)
    {
        var userQuery = Env.Models.Get("Web.ResUsers").NameSearch(name, domain, @operator, limit, order);
        if (limit == null)
        {
            return userQuery;
        }
        var userIds = userQuery.ToList();
        if (Env.Uid != null && userIds.Contains(Env.Uid))
        {
            if (userIds.IndexOf(Env.Uid) != 0)
            {
                userIds.Remove(Env.Uid);
                userIds.Insert(0, Env.Uid);
            }
        }
        else if (limit != null && userIds.Count == limit)
        {
            var newUserIds = Env.Models.Get("Web.ResUsers").NameSearch(
                name,
                Env.Utils.Expression.And(domain ?? new List<object>(), new List<object> { new Dictionary<string, object> { { "Id", Env.Uid } } }),
                @operator,
                limit: 1
            );
            if (newUserIds != null)
            {
                userIds.RemoveAt(userIds.Count - 1);
                userIds.Insert(0, Env.Uid);
            }
        }
        return userIds;
    }
}
