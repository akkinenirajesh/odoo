csharp
public partial class Employee
{
    public void ComputeHasTimesheet()
    {
        var query = @"
            SELECT id, EXISTS(SELECT 1 FROM account_analytic_line WHERE project_id IS NOT NULL AND employee_id = e.id limit 1)
            FROM hr_employee e
            WHERE id in @Ids";

        var parameters = new { Ids = new[] { this.Id } };
        var result = Env.Cr.Query<(int Id, bool HasTimesheet)>(query, parameters).ToDictionary(x => x.Id, x => x.HasTimesheet);

        this.HasTimesheet = result.GetValueOrDefault(this.Id, false);
    }

    public void ComputeDisplayName()
    {
        // Call base implementation first
        base.ComputeDisplayName();

        var allowedCompanyIds = Env.Context.GetValueOrDefault("allowed_company_ids", new List<int>());
        if (allowedCompanyIds.Count <= 1)
        {
            return;
        }

        var employeesCountPerUser = Env.GetModel<Hr.Employee>().Sudo()
            .ReadGroup(
                domain: new[] 
                { 
                    ("UserId", "in", new[] { this.UserId.Id }),
                    ("CompanyId", "in", allowedCompanyIds)
                },
                groupBy: new[] { "UserId" },
                aggregates: new[] { "__count" }
            )
            .ToDictionary(g => g.UserId.Id, g => g.__count);

        if (employeesCountPerUser.GetValueOrDefault(this.UserId.Id, 0) > 1)
        {
            this.DisplayName = $"{this.DisplayName} - {this.CompanyId.Name}";
        }
    }

    public ActionResult ActionUnlinkWizard()
    {
        var wizard = Env.GetModel<Hr.EmployeeDeleteWizard>().Create(new {
            EmployeeIds = new[] { this.Id }
        });

        if (!Env.User.HasGroup("Hr_Timesheet.GroupHrTimesheetApprover") && wizard.HasTimesheet && !wizard.HasActiveEmployee)
        {
            throw new UserError("You cannot delete employees who have timesheets.");
        }

        return new ActionResult
        {
            Name = "Confirmation",
            ViewMode = "form",
            ResModel = "Hr.EmployeeDeleteWizard",
            Views = new[] { (Env.Ref("Hr_Timesheet.HrEmployeeDeleteWizardForm").Id, "form") },
            Type = "ir.actions.act_window",
            ResId = wizard.Id,
            Target = "new",
            Context = Env.Context
        };
    }

    public ActionResult ActionTimesheetFromEmployee()
    {
        var action = Env.GetModel<Ir.ActionsActWindow>().ForXmlId("Hr_Timesheet.TimesheetActionFromEmployee");
        var context = new Dictionary<string, object>(action.Context);
        context["active_id"] = this.Id;
        context["create"] = context.GetValueOrDefault("create", true) && this.Active;
        action.Context = context;
        return action;
    }
}
