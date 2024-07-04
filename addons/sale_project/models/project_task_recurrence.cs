C#
public partial class SaleProjectTaskRecurrence
{
    public virtual IEnumerable<Sale.SaleOrderLine> SaleLineId { get; set; }

    public virtual IEnumerable<Sale.SaleOrderLine> GetRecurringFieldsToCopy()
    {
        var result = Env.Call<IEnumerable<Sale.SaleOrderLine>>("Sale.ProjectTaskRecurrence", "_get_recurring_fields_to_copy", this);
        result = result.Concat(SaleLineId);
        return result;
    }
}
