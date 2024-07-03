csharp
public partial class Stage
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeTeamCount()
    {
        TeamCount = Env.Search<Crm.Team>().Count();
    }

    public override Dictionary<string, object> DefaultGet(IEnumerable<string> fields)
    {
        var result = base.DefaultGet(fields);
        if (Env.Context.ContainsKey("default_team_id"))
        {
            var ctx = new Dictionary<string, object>(Env.Context);
            ctx.Remove("default_team_id");
            Env = Env.WithContext(ctx);
        }
        return result;
    }
}
