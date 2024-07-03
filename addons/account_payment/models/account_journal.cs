csharp
public partial class AccountJournal
{
    public List<PaymentMethodLine> GetAvailablePaymentMethodLines(string paymentType)
    {
        var lines = base.GetAvailablePaymentMethodLines(paymentType);
        return lines.Where(l => l.PaymentProvider.State != "disabled").ToList();
    }

    public void UnlinkExceptLinkedToPaymentProvider()
    {
        var linkedProviders = Env.Set<PaymentProvider>().Search(new List<object>())
            .Where(p => p.Journal.Id == this.Id && p.State != "disabled")
            .ToList();

        if (linkedProviders.Any())
        {
            var providerNames = string.Join(", ", linkedProviders.Select(p => p.DisplayName));
            throw new UserException(
                $"You must first deactivate a payment provider before deleting its journal.\n" +
                $"Linked providers: {providerNames}"
            );
        }
    }
}
