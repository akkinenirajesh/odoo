csharp
public partial class ProjectMilestone {
    public void ToggleIsReached(bool isReached) {
        this.IsReached = isReached;
    }

    public void ActionViewTasks() {
        // TODO: Implement logic to view tasks
    }

    public void ComputeReachedDate() {
        if (this.IsReached) {
            this.ReachedDate = Env.Today;
        }
    }

    public void ComputeIsDeadlineExceeded() {
        this.IsDeadlineExceeded = !this.IsReached && this.Deadline.HasValue && this.Deadline.Value < Env.Today;
    }

    public void ComputeIsDeadlineFuture() {
        this.IsDeadlineFuture = this.Deadline.HasValue && this.Deadline.Value > Env.Today;
    }

    public void ComputeTaskCount() {
        // TODO: Implement logic to count tasks
    }

    public void ComputeCanBeMarkedAsDone() {
        // TODO: Implement logic to check if milestone can be marked as done
    }
}
