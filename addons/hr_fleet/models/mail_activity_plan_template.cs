csharp
public partial class MailActivityPlanTemplate
{
    public void CheckResponsibleHrFleet()
    {
        if (PlanId.ResModel != "hr.employee" && ResponsibleType == ResponsibleType.FleetManager)
        {
            throw new ValidationException("Fleet Manager is limited to Employee plans.");
        }
    }

    public DetermineResponsibleResult DetermineResponsible(object onDemandResponsible, HrEmployee employee)
    {
        if (ResponsibleType == ResponsibleType.FleetManager && PlanId.ResModel == "hr.employee")
        {
            var employeeId = Env.Get<HrEmployee>().Browse(employee.Id);
            var vehicle = employeeId.CarIds.FirstOrDefault();
            string error = null;

            if (vehicle == null)
            {
                error = $"Employee {employeeId.Name} is not linked to a vehicle.";
            }
            else if (vehicle.ManagerId == null)
            {
                error = $"The vehicle of employee {employeeId.Name} is not linked to a fleet manager.";
            }

            return new DetermineResponsibleResult
            {
                Responsible = vehicle?.ManagerId,
                Error = error
            };
        }

        return base.DetermineResponsible(onDemandResponsible, employee);
    }
}

public class DetermineResponsibleResult
{
    public object Responsible { get; set; }
    public string Error { get; set; }
}
