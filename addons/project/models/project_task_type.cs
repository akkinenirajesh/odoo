csharp
public partial class ProjectTaskType {
    public virtual void UnlinkWizard(bool stageView = false) {
        // TODO: Implement UnlinkWizard
    }

    public virtual void Write(Dictionary<string, object> vals) {
        // TODO: Implement Write
    }

    public virtual List<Dictionary<string, object>> CopyData(Dictionary<string, object> defaultValues = null) {
        // TODO: Implement CopyData
    }

    public virtual void UnlinkIfRemainingPersonalStages() {
        // TODO: Implement UnlinkIfRemainingPersonalStages
    }

    public virtual void PreparePersonalStagesDeletion(List<Dictionary<string, object>> remainingStagesDict, List<ProjectTaskStagePersonal> personalStagesToUpdate) {
        // TODO: Implement PreparePersonalStagesDeletion
    }

    public virtual void ToggleActive() {
        // TODO: Implement ToggleActive
    }

    public virtual void ComputeDisabledRatingWarning() {
        // TODO: Implement ComputeDisabledRatingWarning
    }

    public virtual void ComputeUserId() {
        // TODO: Implement ComputeUserId
    }

    public virtual void CheckPersonalStageNotLinkedToProjects() {
        // TODO: Implement CheckPersonalStageNotLinkedToProjects
    }

    private List<Project.Project> GetDefaultProjectIds() {
        // TODO: Implement GetDefaultProjectIds
    }

    private Res.Users DefaultUserId() {
        // TODO: Implement DefaultUserId
    }
}
