csharp
public partial class ProjectProjectStage {

    public void CopyData(Dictionary<string, object> defaultValues)
    {
        var valsList = Env.Call("copy_data", this, defaultValues);
        var updatedValsList = valsList.Select((vals, index) => new Dictionary<string, object>() {
            { "Name", string.Format("{0} (copy)", this[index].Get("Name")) }
        }).ToList();
        return updatedValsList;
    }

    public void UnlinkWizard(bool stageView = false)
    {
        var wizard = Env.Create<ProjectProjectStageDeleteWizard>(new Dictionary<string, object>() { { "StageIds", this.Ids } });
        var context = Env.Context;
        context.Add("StageView", stageView);
        return Env.CreateAction(new Dictionary<string, object>() {
            { "name", "Delete Project Stage" },
            { "view_mode", "form" },
            { "res_model", "Project.ProjectProjectStageDeleteWizard" },
            { "views", new List<object>() { new List<object>() { Env.Ref("project.view_project_project_stage_delete_wizard").Id, "form" } } },
            { "type", "ir.actions.act_window" },
            { "res_id", wizard.Id },
            { "target", "new" },
            { "context", context },
        });
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("Company"))
        {
            var project = Env.Search<ProjectProject>(new List<object>() { new List<object>() { "Stage", this }, new List<object>() { "Company", "!=", vals["Company"] } }, limit: 1);
            if (project.Any())
            {
                var company = Env.Get<ResCompany>(vals["Company"]);
                throw new UserError(
                    string.Format("You are not able to switch the company of this stage to {0} since it currently includes projects associated with {1}. Please ensure that this stage exclusively consists of projects linked to {0}.",
                        company.Get("Name"),
                        project.First().Get("Company") != null ? project.First().Get("Company").Get("Name") : "no company"
                    )
                );
            }
        }

        if (vals.ContainsKey("Active") && !(bool)vals["Active"])
        {
            Env.Search<ProjectProject>(new List<object>() { new List<object>() { "Stage", this } }).Write(new Dictionary<string, object>() { { "Active", false } });
        }

        return Env.Call("write", this, vals);
    }

    public void ToggleActive()
    {
        var res = Env.Call("toggle_active", this);
        var stageActive = this.Where(s => (bool)s.Get("Active"));
        var inactiveProjects = Env.Search<ProjectProject>(new List<object>() { new List<object>() { "Active", false }, new List<object>() { "Stage", stageActive } }, limit: 1);
        if (stageActive.Any() && inactiveProjects.Any())
        {
            var wizard = Env.Create<ProjectProjectStageDeleteWizard>(new Dictionary<string, object>() { { "StageIds", stageActive.Ids } });
            return Env.CreateAction(new Dictionary<string, object>() {
                { "name", "Unarchive Projects" },
                { "view_mode", "form" },
                { "res_model", "Project.ProjectProjectStageDeleteWizard" },
                { "views", new List<object>() { new List<object>() { Env.Ref("project.view_project_project_stage_unarchive_wizard").Id, "form" } } },
                { "type", "ir.actions.act_window" },
                { "res_id", wizard.Id },
                { "target", "new" },
            });
        }
        return res;
    }
}
