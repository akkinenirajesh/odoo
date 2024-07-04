csharp
public partial class AccountMove
{
    public Core.Partner L10nInGetWarehouseAddress()
    {
        var res = base.L10nInGetWarehouseAddress();
        if (InvoiceLineIds.Any(line => line.PurchaseLineId != null))
        {
            var companyShippingId = InvoiceLineIds
                .Where(line => line.PurchaseLineId != null)
                .SelectMany(line => line.PurchaseLineId.MoveIds)
                .Select(move => move.WarehouseId.PartnerId)
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
