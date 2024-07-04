csharp
public partial class Order
{
    public Core.BankAccount GetPartnerBankId()
    {
        var bankPartnerId = base.GetPartnerBankId();
        
        if (this.Company.Country?.Code == "CH")
        {
            var hasPayLater = this.PaymentIds.Any(p => p.PaymentMethodId.JournalId == null);
            bankPartnerId = hasPayLater ? bankPartnerId : null;
        }
        
        return bankPartnerId;
    }
}
