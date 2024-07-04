csharp
public partial class AccountMoveLine
{
    public void _ComputeVatAmount()
    {
        this.L10nAeVatAmount = this.PriceTotal - this.PriceSubtotal;
    }
}
