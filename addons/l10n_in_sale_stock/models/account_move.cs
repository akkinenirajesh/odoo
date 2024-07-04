csharp
public partial class AccountMove
{
    public Core.Partner L10nInGetWarehouseAddress()
    {
        var res = base.L10nInGetWarehouseAddress();
        
        if (InvoiceLineIds.Any(l => l.SaleLineIds.Any()))
        {
            var companyShippingId = InvoiceLineIds
                .SelectMany(l => l.SaleLineIds)
                .Select(sl => sl.Order.Warehouse.Partner)
                .Distinct()
                .ToList();

            if (companyShippingId.Count == 1)
            {
                return companyShippingId.First();
            }
        }

        return res;
    }
}
