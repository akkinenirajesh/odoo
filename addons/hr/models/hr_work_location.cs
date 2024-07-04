csharp
public partial class WorkLocation
{
    public override string ToString()
    {
        return Name;
    }

    public WorkLocation()
    {
        Company = Env.Company;
    }
}
