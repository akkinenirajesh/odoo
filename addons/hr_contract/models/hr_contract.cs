csharp
public partial class Contract
{
    public override string ToString()
    {
        return Name;
    }

    private void _ComputeCalendarMismatch()
    {
        CalendarMismatch = ResourceCalendarId != EmployeeId.ResourceCalendarId;
    }

    private void _ComputeEmployeeContract()
    {
        if (EmployeeId != null)
        {
            JobId = EmployeeId.JobId;
            DepartmentId = EmployeeId.DepartmentId;
            ResourceCalendarId = EmployeeId.ResourceCalendarId;
            CompanyId = EmployeeId.CompanyId;
        }
    }

    private void _ComputeStructureTypeId()
    {
        var defaultStructureByCountry = new Dictionary<int, Hr.PayrollStructureType>();

        Hr.PayrollStructureType _DefaultSalaryStructure(int countryId)
        {
            if (!defaultStructureByCountry.TryGetValue(countryId, out var defaultStructure))
            {
                defaultStructure = Env.Set<Hr.PayrollStructureType>()
                    .Search(x => x.CountryId.Id == countryId)
                    .FirstOrDefault() ?? Env.Set<Hr.PayrollStructureType>()
                    .Search(x => x.CountryId == null)
                    .FirstOrDefault();
                defaultStructureByCountry[countryId] = defaultStructure;
            }
            return defaultStructure;
        }

        if (StructureTypeId == null || StructureTypeId.CountryId != CompanyId.CountryId)
        {
            StructureTypeId = _DefaultSalaryStructure(CompanyId.CountryId.Id);
        }
    }

    private void _ComputeContractWage()
    {
        ContractWage = _GetContractWage();
    }

    private decimal _GetContractWage()
    {
        return Wage;
    }

    // Other methods would be implemented here...
}
