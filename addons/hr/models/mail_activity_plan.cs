csharp
public partial class MailActivityPlan
{
    public void CheckCompatibilityWithModel()
    {
        var planToCheck = Env.Query<MailActivityPlan>().Where(plan => !plan.DepartmentAssignable);
        var failingPlans = planToCheck.Where(plan => plan.DepartmentId != null).ToList();
        if (failingPlans.Any())
        {
            throw new UserException(string.Format("Plan {0} cannot use a department as it is used only for some HR plans.",
                string.Join(", ", failingPlans.Select(p => p.Name))));
        }

        planToCheck = Env.Query<MailActivityPlan>().Where(plan => plan.ResModel != "hr.employee");
        var failingTemplates = planToCheck.SelectMany(p => p.TemplateIds)
            .Where(tpl => new[] { "coach", "manager", "employee" }.Contains(tpl.ResponsibleType))
            .ToList();
        if (failingTemplates.Any())
        {
            throw new UserException(string.Format("Plan activities {0} cannot use coach, manager or employee responsible as it is used only for employee plans.",
                string.Join(", ", failingTemplates.Select(t => t.ActivityTypeId.Name))));
        }
    }

    public void ComputeDepartmentAssignable()
    {
        DepartmentAssignable = ResModel == "hr.employee";
    }

    public void ComputeDepartmentId()
    {
        if (!DepartmentAssignable)
        {
            DepartmentId = null;
        }
    }
}
