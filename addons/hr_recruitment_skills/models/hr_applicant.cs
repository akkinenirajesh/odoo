csharp
public partial class HrApplicant
{
    public bool ComputeIsInterviewer()
    {
        var isRecruiter = Env.User.HasGroup("hr_recruitment.group_hr_recruitment_user");
        return !isRecruiter && (InterviewerIds.Contains(Env.User) || JobId.InterviewerIds.Contains(Env.User));
    }

    public void ComputeSkillIds()
    {
        SkillIds = ApplicantSkillIds.Select(s => s.SkillId).ToList();
    }

    public void ComputeMatchingSkillIds()
    {
        var jobId = Env.Context.GetValueOrDefault("ActiveId", 0);
        if (jobId == 0)
        {
            MatchingSkillIds = new List<HrSkill>();
            MissingSkillIds = new List<HrSkill>();
            MatchingScore = 0;
        }
        else
        {
            var job = Env.Get<HrJob>().Browse(jobId);
            var jobSkills = job.SkillIds;
            MatchingSkillIds = jobSkills.Intersect(SkillIds).ToList();
            MissingSkillIds = jobSkills.Except(SkillIds).ToList();
            MatchingScore = jobSkills.Any() ? (decimal)MatchingSkillIds.Count / jobSkills.Count * 100 : 0;
        }
    }

    public Dictionary<string, object> GetEmployeeCreateVals()
    {
        var vals = base.GetEmployeeCreateVals();
        vals["EmployeeSkillIds"] = ApplicantSkillIds.Select(applicantSkill => new Dictionary<string, object>
        {
            ["SkillId"] = applicantSkill.SkillId.Id,
            ["SkillLevelId"] = applicantSkill.SkillLevelId.Id,
            ["SkillTypeId"] = applicantSkill.SkillTypeId.Id
        }).ToList();
        return vals;
    }

    public void UpdateEmployeeFromApplicant()
    {
        var valsToCreate = new List<Dictionary<string, object>>();
        var existingSkills = EmpId.EmployeeSkillIds.Select(s => s.SkillId).ToList();
        var skillsToCreate = ApplicantSkillIds.Select(s => s.SkillId).Except(existingSkills);

        foreach (var skill in skillsToCreate)
        {
            var applicantSkill = ApplicantSkillIds.First(s => s.SkillId == skill);
            valsToCreate.Add(new Dictionary<string, object>
            {
                ["EmployeeId"] = EmpId.Id,
                ["SkillId"] = skill.Id,
                ["SkillLevelId"] = applicantSkill.SkillLevelId.Id,
                ["SkillTypeId"] = skill.SkillTypeId.Id
            });
        }

        Env.Get<HrEmployeeSkill>().Create(valsToCreate);
        base.UpdateEmployeeFromApplicant();
    }

    public Dictionary<string, object> ActionAddToJob()
    {
        var jobId = Env.Context.GetValueOrDefault("ActiveId", 0);
        var job = Env.Get<HrJob>().Browse(jobId);
        var stage = Env.Ref<HrRecruitmentStage>("hr_recruitment.stage_job0");

        this.WithContext(new Dictionary<string, object> { ["JustMoved"] = true }).Write(new Dictionary<string, object>
        {
            ["JobId"] = job.Id,
            ["StageId"] = stage.Id
        });

        var action = Env.Get<IrActionsActions>().ForXmlId("hr_recruitment.action_hr_job_applications");
        action["Context"] = action["Context"].ToString().Replace("ActiveId", JobId.Id.ToString());
        return action;
    }
}
