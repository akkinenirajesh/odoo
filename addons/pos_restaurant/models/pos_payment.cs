C#
public partial class PosPayment
{
    public void UpdatePaymentLineForTip(decimal tipAmount)
    {
        this.Amount = this.Amount + tipAmount;
        this.Save();
    }
}
