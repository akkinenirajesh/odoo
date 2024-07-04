csharp
public partial class ResumeLine
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<ResumeLine> GetEmployeeResumeLines()
    {
        return Env.Query<ResumeLine>()
            .Where(r => r.Employee == this.Employee)
            .OrderBy(r => r.LineType)
            .ThenByDescending(r => r.DateEnd)
            .ThenByDescending(r => r.DateStart)
            .ToList();
    }
}
