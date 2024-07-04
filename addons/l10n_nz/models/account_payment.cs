csharp
public partial class AccountPayment {
    public void ComputePaymentReceiptTitle() {
        if (Env.Context.Contains("active_ids") && this.Country_Code == "NZ" && this.Partner_Type == "supplier") {
            this.Payment_Receipt_Title = Env.Translate("Remittance Advice");
        }
    }
}
