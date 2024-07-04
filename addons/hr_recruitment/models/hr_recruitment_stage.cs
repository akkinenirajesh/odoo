csharp
public partial class RecruitmentStage
{
    public override string ToString()
    {
        return Name;
    }

    public Dictionary<string, object> DefaultGet(string[] fields)
    {
        var result = new Dictionary<string, object>();
        var context = Env.Context;

        if (context.ContainsKey("default_job_id") && !context.ContainsKey("hr_recruitment_stage_mono"))
        {
            var newContext = new Dictionary<string, object>(context);
            newContext.Remove("default_job_id");
            Env = Env.WithContext(newContext);
        }

        // Call the base implementation to get default values
        var baseDefaults = base.DefaultGet(fields);
        foreach (var field in fields)
        {
            if (baseDefaults.ContainsKey(field))
            {
                result[field] = baseDefaults[field];
            }
        }

        return result;
    }

    public void ComputeIsWarningVisible()
    {
        var applicantData = Env.Set<Hr.Applicant>().ReadGroup(
            domain: new[] { ("Stage", "in", new[] { this.Id }) },
            fields: new[] { "Stage" },
            groupBy: new[] { "Stage" }
        );

        var applicants = applicantData.ToDictionary(
            group => group.Stage.Id,
            group => group.Count
        );

        if (Origin.HiredStage && !HiredStage && applicants.ContainsKey(Origin.Id))
        {
            IsWarningVisible = true;
        }
        else
        {
            IsWarningVisible = false;
        }
    }
}
