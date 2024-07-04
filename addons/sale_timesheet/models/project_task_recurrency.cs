C#
public partial class ProjectTaskRecurrence
{
    public virtual List<ProjectTaskRecurrence> GetRecurringFieldsToCopy()
    {
        var result = Env.GetDefaultModel("Project.ProjectTaskRecurrence").CallMethod<List<ProjectTaskRecurrence>>("_GetRecurringFieldsToCopy");
        result.Add(Env.GetDefaultModel("Project.ProjectTaskRecurrence").GetField("SoAnalyticAccountId"));
        return result;
    }
}
