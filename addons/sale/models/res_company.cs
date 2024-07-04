csharp
public partial class ResCompany {
    public void CheckPrepaymentPercent() {
        if (this.PortalConfirmationPay && !(0 < this.PrepaymentPercent && this.PrepaymentPercent <= 1.0)) {
            throw new Exception("Prepayment percentage must be a valid percentage.");
        }
    }
}
