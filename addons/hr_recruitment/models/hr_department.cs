csharp
public partial class Department
{
    public int ComputeNewApplicantCount()
    {
        if (Env.User.HasGroup("Hr.Recruitment.GroupHrRecruitmentInterviewer"))
        {
            var applicantData = Env.Set<Hr.Applicant>().ReadGroup(
                new[] { ("Department", "in", new[] { this.Id }), ("Stage.Sequence", "<=", 1) },
                new[] { "Department" },
                new[] { "__count" }
            );

            var result = applicantData.ToDictionary(
                item => item.Department.Id,
                item => item.__count
            );

            return result.GetValueOrDefault(this.Id, 0);
        }
        else
        {
            return 0;
        }
    }

    public (int NewHiredEmployee, int ExpectedEmployee) ComputeRecruitmentStats()
    {
        var jobData = Env.Set<Hr.Job>().ReadGroup(
            new[] { ("Department", "in", new[] { this.Id }) },
            new[] { "Department" },
            new[] { "NoOfHiredEmployee:sum", "NoOfRecruitment:sum" }
        );

        var newEmp = jobData.ToDictionary(
            item => item.Department.Id,
            item => item.NoOfHiredEmployee
        );

        var expectedEmp = jobData.ToDictionary(
            item => item.Department.Id,
            item => item.NoOfRecruitment
        );

        return (
            NewHiredEmployee: newEmp.GetValueOrDefault(this.Id, 0),
            ExpectedEmployee: expectedEmp.GetValueOrDefault(this.Id, 0)
        );
    }
}
