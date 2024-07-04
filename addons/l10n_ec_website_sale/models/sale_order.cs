csharp
public partial class SaleOrder
{
    public List<Account.Move> _CreateInvoices(bool grouped = false, bool final = false, DateTime? date = null)
    {
        // Call the base implementation
        var moves = base._CreateInvoices(grouped, final, date);

        foreach (var move in moves)
        {
            if (move.TransactionIds.Any())
            {
                var sriPaymentMethods = move.TransactionIds
                    .Select(t => t.PaymentMethodId.L10nEcSriPaymentId)
                    .Distinct()
                    .ToList();

                if (sriPaymentMethods.Count == 1)
                {
                    move.L10nEcSriPaymentId = sriPaymentMethods.First();
                }
            }
        }

        return moves;
    }
}
