csharp
public partial class LeaveType
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<Leave> PopulateLeaves(int size)
    {
        var random = new Random();
        var employees = Env.Get<HR.Employee>().GetAll();
        var leaveTypes = Env.Get<HR.LeaveType>().GetAll().Where(lt => lt.RequiresAllocation == LeaveTypeAllocation.No);

        var employeesByCompany = employees.GroupBy(e => e.Company.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (int i = 0; i < size; i++)
        {
            var leaveType = leaveTypes.RandomElement();
            var company = leaveType.Company;
            var employee = employeesByCompany[company.Id].RandomElement();

            var startDate = DateTime.Today.AddDays(3 * i);
            var endDate = startDate.AddDays(random.Next(0, 3));

            yield return new Leave
            {
                HolidayStatus = leaveType,
                Employee = employee,
                RequestDateFrom = startDate,
                RequestDateTo = endDate,
                State = random.NextDouble() < 0.5 ? LeaveState.Draft : LeaveState.Confirm
            };
        }
    }
}
