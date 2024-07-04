csharp
public partial class ResPartner {
    public void ComputePaymentTokenCount() {
        var paymentsData = Env.GetModel("Payment.PaymentToken").ReadGroup(new[] { new SearchDomain("Partner", this.Id, "=", null) }, new[] { "Partner" }, new[] { "__count" });
        var partnersData = paymentsData.ToDictionary(p => p.Partner, p => p.__count);
        this.PaymentTokenCount = partnersData.ContainsKey(this.Id) ? partnersData[this.Id] : 0;
    }
}
