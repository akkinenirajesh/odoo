csharp
public partial class AccountPayment
{
    public void ComputePaymentReceiptTitle()
    {
        // Call the base implementation first
        base.ComputePaymentReceiptTitle();

        if (this.CountryCode == "AU" && this.PartnerType == "supplier")
        {
            this.PaymentReceiptTitle = Env.T("Remittance Advice");
        }
    }
}
