C#
public partial class LeadTest
{
  public void ComputeStageId()
  {
    var Stage = Env.GetModel<Stage>();
    if (this.StageId == null && this.State == "draft")
    {
      this.StageId = Stage.Search(x => x.Name.Contains("new"), 1) ?? Stage.Create(new Stage { Name = "New" });
    }
  }

  public void ComputeEmployeeDeadline()
  {
    this.Employee = this.PartnerId.Employee;
    if (!this.Priority)
    {
      this.Deadline = false;
    }
    else
    {
      // Need to convert this.CreateDate to DateTime object and add relativedelta
      // this.Deadline = this.CreateDate + relativedelta.relativedelta(days=3);
    }
  }

  public void Write(Dictionary<string, object> vals)
  {
    var result = base.Write(vals);
    this.Employee = this.Employee; // force recomputation of field 'Deadline' via 'Employee'
    return result;
  }
}

public partial class Task
{
  public void ComputeProjectId()
  {
    if (this.ProjectId == null)
    {
      this.ProjectId = this.ParentId.ProjectId;
    }
  }
}
