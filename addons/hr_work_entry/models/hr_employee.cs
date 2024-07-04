csharp
public partial class Employee
{
    public bool ComputeHasWorkEntries()
    {
        var query = @"
            SELECT EXISTS(SELECT 1 FROM Hr.WorkEntry WHERE Employee = @p0 LIMIT 1)
        ";
        var result = Env.Cr.ExecuteScalar<bool>(query, this.Id);
        return result;
    }

    public ActionResult ActionOpenWorkEntries(DateTime? initialDate = null)
    {
        var ctx = new Dictionary<string, object>
        {
            ["DefaultEmployee"] = this.Id
        };

        if (initialDate.HasValue)
        {
            ctx["InitialDate"] = initialDate.Value;
        }

        return new ActionResult
        {
            Type = ActionType.Window,
            Name = $"{this.DisplayName} work entries",
            ViewMode = "calendar,tree,form",
            Model = "Hr.WorkEntry",
            Context = ctx,
            Domain = new List<object> { new List<object> { "Employee", "=", this.Id } }
        };
    }
}
