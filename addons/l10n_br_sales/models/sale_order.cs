csharp
public partial class SaleOrder
{
    public string GetNamePortalContentView()
    {
        return this.Company?.Country?.Code == "BR" 
            ? "l10n_br_sales.sale_order_portal_content_brazil" 
            : base.GetNamePortalContentView();
    }

    public string GetNameTaxTotalsView()
    {
        return this.Company?.Country?.Code == "BR" 
            ? "l10n_br_sales.document_tax_totals_brazil" 
            : base.GetNameTaxTotalsView();
    }
}
