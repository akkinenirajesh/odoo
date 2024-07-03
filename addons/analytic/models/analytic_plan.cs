csharp
public partial class AccountAnalyticPlan
{
    public override string ToString()
    {
        return CompleteName;
    }

    private int DefaultColor()
    {
        return new Random().Next(1, 12);
    }

    public void ComputeRootId()
    {
        if (!string.IsNullOrEmpty(ParentPath))
        {
            var rootId = int.Parse(ParentPath.TrimEnd('/').Split('/')[0]);
            RootId = Env.Get<AccountAnalyticPlan>(rootId);
        }
        else
        {
            RootId = this;
        }
    }

    public void ComputeCompleteName()
    {
        if (ParentId != null)
        {
            CompleteName = $"{ParentId.CompleteName} / {Name}";
        }
        else
        {
            CompleteName = Name;
        }
    }

    public void ComputeAnalyticAccountCount()
    {
        AccountCount = AccountIds.Count();
    }

    public void ComputeAllAnalyticAccountCount()
    {
        // This method would require a more complex implementation in C#
        // as it involves database operations that are not directly translatable
    }

    public void ComputeChildrenCount()
    {
        ChildrenCount = ChildrenIds.Count();
    }

    public ActionResult ActionViewAnalyticalAccounts()
    {
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            ResModel = "Account.AnalyticAccount",
            Domain = new List<object> { new List<object> { "PlanId", "child_of", Id } },
            Context = new Dictionary<string, object> { { "default_plan_id", Id } },
            Name = "Analytical Accounts",
            ViewMode = "list,form"
        };
    }

    public ActionResult ActionViewChildrenPlans()
    {
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            ResModel = "Analytic.AccountAnalyticPlan",
            Domain = new List<object> { new List<object> { "ParentId", "=", Id } },
            Context = new Dictionary<string, object>
            {
                { "default_parent_id", Id },
                { "default_color", Color }
            },
            Name = "Analytical Plans",
            ViewMode = "list,form"
        };
    }

    // Other methods would need to be implemented similarly,
    // translating Odoo's ORM operations to C# equivalents
}
