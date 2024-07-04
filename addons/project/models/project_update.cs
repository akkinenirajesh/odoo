C#
public partial class ProjectUpdate {
    public ProjectUpdate() { }

    public void ComputeColor() {
        this.Color = STATUS_COLOR[this.Status];
    }

    public void ComputeProgressPercentage() {
        this.ProgressPercentage = this.Progress / 100;
    }

    public void ComputeNameCropped() {
        if (this.Name.Length > 60) {
            this.NameCropped = this.Name.Substring(0, 57) + "...";
        } else {
            this.NameCropped = this.Name;
        }
    }

    public void ComputeClosedTaskPercentage() {
        this.ClosedTaskPercentage = this.TaskCount > 0 ? Math.Round((double)this.ClosedTaskCount / this.TaskCount * 100) : 0;
    }

    // ... more methods for unlink, create, _build_description, _get_template_values, _get_milestone_values, _get_last_updated_milestone etc.
}
