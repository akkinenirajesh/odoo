csharp
public partial class StockMove
{
    public decimal L10nInGetProductPriceUnit()
    {
        var product = this.ProductId;
        var company = this.CompanyId;
        var standardPrice = product.WithCompany(company).StandardPrice;
        return product.UomId.ComputePrice(standardPrice, this.ProductUom);
    }

    public Dictionary<string, object> L10nInGetProductTax()
    {
        return new Dictionary<string, object>
        {
            { "IsFromOrder", false },
            { "Taxes", this.PickingCode == "incoming" ? 
                this.ProductId.SupplierTaxesId : 
                this.ProductId.TaxesId }
        };
    }
}
