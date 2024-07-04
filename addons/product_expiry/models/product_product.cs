csharp
public partial class Product
{
    public virtual object ActionOpenQuants()
    {
        // Override to hide the `removal_date` column if not needed.
        if (!Env.Context.GetBool("hide_removal_date"))
        {
            if (!this.UseExpirationDate)
            {
                Env.Context.Set("hide_removal_date", true);
            }
        }
        return Env.Call("product.product", "action_open_quants");
    }
}
