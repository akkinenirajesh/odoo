csharp
public partial class AfipResponsibilityType
{
    public override string ToString()
    {
        return Name;
    }

    public static IEnumerable<AfipResponsibilityType> GetAll(BuviEnvironment env)
    {
        return env.Query<AfipResponsibilityType>().OrderBy(a => a.Sequence);
    }
}
