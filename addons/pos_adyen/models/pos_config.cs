csharp
public partial class PosConfig 
{
    public void CheckAdyenAskCustomerForTip()
    {
        if (this.AdyenAskCustomerForTip && (this.TipProductId == null || !this.IfaceTipProduct))
        {
            throw new Exception($"Please configure a tip product for POS {this.Name} to support tipping with Adyen.");
        }
    }
}
