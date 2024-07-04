csharp
public partial class StockPicking
{
    public Partner GetL10nInDropshipDestPartner()
    {
        if (this.PurchaseId != null)
        {
            return this.PurchaseId.DestAddressId;
        }
        return null;
    }

    public FiscalPosition L10nInGetFiscalPosition()
    {
        if (this.PurchaseId != null)
        {
            return this.PurchaseId.FiscalPositionId;
        }
        return base.L10nInGetFiscalPosition();
    }
}
