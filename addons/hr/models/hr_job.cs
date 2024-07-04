csharp
public partial class Job
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeEmployees()
    {
        var employeeData = Env.Get<Hr.Employee>().ReadGroup(
            new[] { ("Job", "=", this.Id) },
            new[] { "Job" },
            new[] { "__count" }
        );

        int count = employeeData.FirstOrDefault()?.Count ?? 0;
        NoOfEmployee = count;
        ExpectedEmployees = count + NoOfRecruitment;
    }

    public override Job Create()
    {
        // We don't want the current user to be follower of all created job
        return base.Create(new { MailCreateNosubscribe = true });
    }

    public override Job Copy(Dictionary<string, object> defaultValues = null)
    {
        var copiedJob = base.Copy(defaultValues);
        copiedJob.Name = $"{Name} (copy)";
        return copiedJob;
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("Description"))
        {
            Env.Get<Web.Editor.Tools>().HandleHistoryDivergence(this, "Description", vals);
        }
        return base.Write(vals);
    }
}
