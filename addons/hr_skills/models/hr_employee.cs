csharp
public partial class Employee
{
    public IEnumerable<Hr.Skill> ComputeSkillIds()
    {
        return EmployeeSkillIds.Select(es => es.SkillId);
    }

    public override void OnCreate()
    {
        base.OnCreate();

        if (Env.Context.Get<bool>("salary_simulation"))
            return;

        var lineType = Env.Ref<Hr.ResumeType>("hr_skills.resume_type_experience");
        var resumeLine = new Hr.ResumeLine
        {
            EmployeeId = this.Id,
            Name = this.CompanyId?.Name ?? "",
            DateStart = this.CreateDate.Date,
            Description = this.JobTitle ?? "",
            LineTypeId = lineType?.Id
        };

        Env.Create(resumeLine);
    }

    public override void OnWrite(IDictionary<string, object> values)
    {
        base.OnWrite(values);

        if (values.ContainsKey("DepartmentId"))
        {
            foreach (var skill in EmployeeSkillIds)
            {
                skill.CreateLogs();
            }
        }
    }
}
