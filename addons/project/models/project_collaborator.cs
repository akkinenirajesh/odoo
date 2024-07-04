csharp
public partial class ProjectCollaborator
{
    public ProjectCollaborator() { }

    public void ComputeDisplayName() 
    {
        this.DisplayName = $"{Env.Get("Project.Project").Get(this.ProjectId).DisplayName} - {Env.Get("Res.Partner").Get(this.PartnerId).DisplayName}";
    }

    public ProjectCollaborator Create(Dictionary<string, object> vals)
    {
        var collaborator = Env.Get("Project.ProjectCollaborator").Search([], 1);
        if (collaborator == null)
        {
            this._ToggleProjectSharingPortalRules(true);
        }
        return Env.Get("Project.ProjectCollaborator").Create(vals);
    }

    public void Unlink()
    {
        var collaborator = Env.Get("Project.ProjectCollaborator").Search([], 1);
        if (collaborator == null)
        {
            this._ToggleProjectSharingPortalRules(false);
        }
    }

    public void _ToggleProjectSharingPortalRules(bool active)
    {
        var accessProjectSharingPortal = Env.Get("Ir.Model.Access").Get("project.access_project_sharing_task_portal").As<IrModelAccess>();
        if (accessProjectSharingPortal.Active != active)
        {
            accessProjectSharingPortal.Active = active;
            accessProjectSharingPortal.Save();
        }

        var taskPortalIrRule = Env.Get("Ir.Rule").Get("project.project_task_rule_portal_project_sharing").As<IrRule>();
        if (taskPortalIrRule.Active != active)
        {
            taskPortalIrRule.Active = active;
            taskPortalIrRule.Save();
        }
    }
}
