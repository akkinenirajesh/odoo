csharp
public partial class AccountMove
{
    public void InvoicePaidHook()
    {
        // Call the base implementation (equivalent to super() in Python)
        base.InvoicePaidHook();

        // Update event booths
        var saleLineIds = this.LineIds.SelectMany(line => line.SaleLineIds);
        foreach (var saleLine in saleLineIds)
        {
            saleLine.UpdateEventBooths(setPaid: true);
        }
    }
}
