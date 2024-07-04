csharp
public partial class StockPicking
{
    public override bool ShouldGenerateCommercialInvoice()
    {
        base.ShouldGenerateCommercialInvoice();
        return true;
    }

    public virtual Core.Partner GetL10nInDropshipDestPartner()
    {
        // To be overridden by `l10n_in_purchase_stock`
        // Returns destination partner from purchase_id
        return null;
    }

    public virtual Core.Partner L10nInGetInvoicePartner()
    {
        // To be overridden by `l10n_in_sale_stock`
        // Returns invoice partner from sale_id
        return null;
    }

    public virtual Account.FiscalPosition L10nInGetFiscalPosition()
    {
        // To be inherited by `l10n_in_*_stock`
        // Returns fiscal position from order
        return null;
    }
}
