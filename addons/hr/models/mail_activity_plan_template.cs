csharp
public partial class ActivityPlanTemplate
{
    public void CheckResponsibleHR()
    {
        if (Env.GetModel<HumanResources.Employee>().Name != PlanId.ResModel && 
            (ResponsibleType == ResponsibleType.Coach || 
             ResponsibleType == ResponsibleType.Manager || 
             ResponsibleType == ResponsibleType.Employee))
        {
            throw new ValidationException("Those responsible types are limited to Employee plans.");
        }
    }

    public Dictionary<string, object> DetermineResponsible(User onDemandResponsible, HumanResources.Employee employee)
    {
        if (Env.GetModel<HumanResources.Employee>().Name != PlanId.ResModel || 
            (ResponsibleType != ResponsibleType.Coach && 
             ResponsibleType != ResponsibleType.Manager && 
             ResponsibleType != ResponsibleType.Employee))
        {
            // Call base class implementation
            return base.DetermineResponsible(onDemandResponsible, employee);
        }

        string error = null;
        User responsible = null;

        switch (ResponsibleType)
        {
            case ResponsibleType.Coach:
                if (employee.CoachId == null)
                {
                    error = $"Coach of employee {employee.Name} is not set.";
                }
                responsible = employee.CoachId?.UserId;
                if (employee.CoachId != null && responsible == null)
                {
                    error = $"The user of {employee.Name}'s coach is not set.";
                }
                break;

            case ResponsibleType.Manager:
                if (employee.ParentId == null)
                {
                    error = $"Manager of employee {employee.Name} is not set.";
                }
                responsible = employee.ParentId?.UserId;
                if (employee.ParentId != null && responsible == null)
                {
                    error = $"The manager of {employee.Name} should be linked to a user.";
                }
                break;

            case ResponsibleType.Employee:
                responsible = employee.UserId;
                if (responsible == null)
                {
                    error = $"The employee {employee.Name} should be linked to a user.";
                }
                break;
        }

        if (error != null || responsible != null)
        {
            return new Dictionary<string, object>
            {
                { "responsible", responsible },
                { "error", error }
            };
        }

        return null;
    }
}
