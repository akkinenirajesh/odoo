csharp
public partial class SaleOrderLine {
    public virtual decimal Margin { get; set; }
    public virtual decimal MarginPercent { get; set; }
    public virtual decimal PurchasePrice { get; set; }

    public virtual void ComputePurchasePrice() {
        if (this.ProductId == null) {
            this.PurchasePrice = 0.0m;
            return;
        }
        var company = Env.Company.Get(this.CompanyId);
        var line = this.WithCompany(company);
        var productCost = this.ProductId.UomId.ComputePrice(this.ProductId.StandardPrice, this.ProductUom);
        this.PurchasePrice = this.ConvertToSolCurrency(productCost, this.ProductId.CostCurrencyId);
    }

    public virtual void ComputeMargin() {
        this.Margin = this.PriceSubtotal - (this.PurchasePrice * this.ProductUomQty);
        this.MarginPercent = this.PriceSubtotal != 0 ? this.Margin / this.PriceSubtotal : 0;
    }

    public virtual decimal ConvertToSolCurrency(decimal amount, long currencyId) {
        // Implement currency conversion logic here
        return amount;
    }
}
