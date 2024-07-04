csharp
public partial class Picking
{
    public Partner L10nInGetInvoicePartner()
    {
        if (this.SaleId != null)
        {
            return this.SaleId.PartnerInvoiceId;
        }
        return null;
    }

    public FiscalPosition L10nInGetFiscalPosition()
    {
        if (this.SaleId != null)
        {
            return this.SaleId.FiscalPositionId;
        }
        return base.L10nInGetFiscalPosition();
    }
}
