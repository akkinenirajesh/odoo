csharp
public partial class SaleOrder {

    public void ActionConfirm()
    {
        var context = new Dictionary<string, object>(this.Env.Context);
        context.Remove("DefaultTagIds");
        this.Env.Context = context;
        this.ActionConfirm();
    }
}
