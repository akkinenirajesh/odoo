csharp
public partial class PayrollStructureType
{
    public override string ToString()
    {
        return Name;
    }

    public PayrollStructureType()
    {
        DefaultResourceCalendar = Env.Company.ResourceCalendar;
        Country = Env.Company.Country;
    }
}
