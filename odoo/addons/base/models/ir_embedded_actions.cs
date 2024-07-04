C#
public partial class BaseIrEmbeddedActions {
    public BaseIrEmbeddedActions() { }

    public void ComputeIsDeletable() {
        // Implementation for computing IsDeletable
        this.IsDeletable = Env.Get("BaseIrEmbeddedActions").Search(this.Id).GetExternalIds().All(exId => exId.StartsWith("__export__") || exId.StartsWith("__custom__"));
    }

    public void ComputeIsVisible() {
        // Implementation for computing IsVisible
        var activeId = Env.Context.Get("active_id");
        if (activeId == null) {
            this.IsVisible = false;
            return;
        }

        var domainId = new List<object> { "id", "=", activeId };
        var records = Env.Get("BaseIrEmbeddedActions").Search(new List<object> { "ParentResModel", "=", this.ParentResModel });
        var activeModelRecord = Env.Get(this.ParentResModel).Search(domainId, new List<object> { "id" });
        if (this.GroupsIds == null || (this.GroupsIds & Env.User.GroupsId).Any()) {
            var domainModel = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(this.Domain);
            this.IsVisible = (this.ParentResId == null || this.ParentResId == activeId) && this.UserId == Env.Uid && activeModelRecord.FilteredDomain(domainModel).Any();
        } else {
            this.IsVisible = false;
        }
    }

    public virtual void UnlinkIfActionDeletable() {
        // Implementation for UnlinkIfActionDeletable
        if (!this.IsDeletable) {
            throw new System.Exception("You cannot delete a default embedded action");
        }
    }

    public List<string> GetReadableFields() {
        // Implementation for GetReadableFields
        return new List<string>() { "Name", "ParentActionId", "ParentResId", "ParentResModel", "ActionId", "PythonMethod", "UserId", "IsDeletable", "DefaultViewMode", "FilterIds", "Domain", "Context", "GroupsIds" };
    }
}
