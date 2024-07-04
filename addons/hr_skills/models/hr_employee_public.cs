csharp
public partial class EmployeePublic
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base class or this class
        return Name;
    }

    public IEnumerable<Hr.ResumeLine> GetActiveResumeLines()
    {
        return ResumeLineIds.Where(r => r.Active);
    }

    public IEnumerable<Hr.EmployeeSkill> GetActiveSkills()
    {
        return EmployeeSkillIds.Where(s => s.SkillType.Active);
    }
}
