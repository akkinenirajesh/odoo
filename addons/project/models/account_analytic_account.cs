C#
public partial class AccountAnalyticAccount
{
    public virtual int ProjectCount { get; set; }

    public virtual IEnumerable<Project.Project> Projects { get; set; }

    public void ComputeProjectCount()
    {
        var projectData = Env.Get("Project.Project").ReadGroup(new[] { new SearchDomain("AnalyticAccountID", "in", this.Id) }, new[] { "AnalyticAccountID" }, new[] { "__count" });
        var mapping = projectData.Select(x => new { AnalyticAccountID = x[0], Count = x[1] }).ToDictionary(x => x.AnalyticAccountID, x => x.Count);
        this.ProjectCount = mapping.GetValueOrDefault(this.Id, 0);
    }

    public void UnlinkExceptExistingTasks()
    {
        var projects = Env.Get("Project.Project").Search(new[] { new SearchDomain("AnalyticAccountID", "in", this.Id) });
        var hasTasks = Env.Get("Project.Task").SearchCount(new[] { new SearchDomain("ProjectID", "in", projects.Select(p => p.Id)) });
        if (hasTasks > 0)
        {
            throw new UserError("Please remove existing tasks in the project linked to the accounts you want to delete.");
        }
    }

    public void ActionViewProjects()
    {
        var kanbanViewId = Env.Get("project.project").GetViewRef("view_project_kanban");
        var action = new Action("project.project", new[] { new View(kanbanViewId, "kanban"), new View(null, "form") },
            new SearchDomain("AnalyticAccountID", "=", this.Id), new Dictionary<string, object> { { "create", false } }, "Projects");
        if (Projects.Count() == 1)
        {
            action.Views = new[] { new View(null, "form") };
            action.ResId = Projects.First().Id;
        }
        Env.Action(action);
    }
}
