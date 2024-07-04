csharp
public partial class ResPartner {
    public void EnsureSameCompanyThanProjects() {
        if (this.CompanyId != null && this.ProjectIds.Any(p => p.CompanyId != this.CompanyId)) {
            throw new Exception("Partner company cannot be different from its assigned projects' company");
        }
    }

    public void EnsureSameCompanyThanTasks() {
        if (this.CompanyId != null && this.TaskIds.Any(t => t.CompanyId != this.CompanyId)) {
            throw new Exception("Partner company cannot be different from its assigned tasks' company");
        }
    }

    public void ComputeTaskCount() {
        var allPartners = Env.Context.Get<bool>("ActiveTest") ? this : Env.Model<ResPartner>().Search(x => x.Id.IsChildOf(this.Id));
        var taskData = Env.Model<Project.Task>().ReadGroup(
            x => x.PartnerId.IsIn(allPartners.Select(p => p.Id)),
            x => x.PartnerId,
            x => x.Count());

        foreach (var task in taskData) {
            var partner = task.Key as ResPartner;
            while (partner != null) {
                if (partner.Id == this.Id) {
                    this.TaskCount += task.Value;
                }
                partner = partner.ParentId;
            }
        }
    }

    public void ViewTasks() {
        var action = Env.Model<Ir.Actions.Actions>().GetForXmlId("project.project_task_action_from_partner");
        action.DisplayName = $"{this.Name}'s Tasks";
        action.Context = new { DefaultPartnerId = this.Id };
        var allChild = Env.Context.Get<bool>("ActiveTest") ? this : Env.Model<ResPartner>().Search(x => x.Id.IsChildOf(this.Id));
        var searchDomain = x => x.PartnerId.IsIn((this | allChild).Select(p => p.Id));
        if (this.TaskCount <= 1) {
            var taskId = Env.Model<Project.Task>().Search(searchDomain).FirstOrDefault()?.Id;
            action.ResId = taskId;
            action.Views = action.Views.Where(v => v.ViewType == "form").ToList();
        } else {
            action.Domain = searchDomain;
        }
    }
}
