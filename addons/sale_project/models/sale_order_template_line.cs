csharp
public partial class SaleOrderTemplateLine {
    public SaleOrderTemplateLine() {
    }

    public virtual SaleOrderTemplateLine _PrepareOrderLineValues() {
        var res = Env.Call("SaleOrderTemplateLine", "_PrepareOrderLineValues", this);
        if (Env.Context.ContainsKey("DefaultTaskId") && this.ProductId.ServiceTracking in new string[] { "TaskInProject", "TaskGlobalProject" }) {
            res.TaskId = null;
        }
        return res;
    }
}
