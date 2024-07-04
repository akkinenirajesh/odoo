C#
public partial class SaleOrderLine {
    public int PurchaseLineCount { get; set; }

    public void ComputePurchaseCount() {
        // Implement logic to calculate PurchaseLineCount based on PurchaseLineIds
        // Example:
        this.PurchaseLineCount = Env.GetModel<PurchaseOrderLine>().Search(l => l.SaleLineId == this.Id).Count();
    }

    public void OnChangeServiceProductUomQty() {
        if (this.State == "sale" && this.ProductId.Type == "service" && this.ProductId.ServiceToPurchase) {
            if (this.ProductUomQty < this.Origin.ProductUomQty) {
                if (this.ProductUomQty < this.QtyDelivered) {
                    return;
                }
                Env.Notify(new WarningMessage { Title = "Ordered quantity decreased!", Message = "You are decreasing the ordered quantity! Do not forget to manually update the purchase order if needed." });
            }
        }
    }

    public void Write(Dictionary<string, object> values) {
        var increasedLines = new List<SaleOrderLine>();
        var decreasedLines = new List<SaleOrderLine>();
        var increasedValues = new Dictionary<int, double>();
        var decreasedValues = new Dictionary<int, double>();

        if (values.ContainsKey("ProductUomQty")) {
            var precision = Env.GetModel<DecimalPrecision>().GetPrecision("Product Unit of Measure");
            increasedLines = this.Filtered(r => r.ProductId.ServiceToPurchase && r.PurchaseLineCount > 0 && Env.Compare(r.ProductUomQty, (double)values["ProductUomQty"], precision) == -1);
            decreasedLines = this.Filtered(r => r.ProductId.ServiceToPurchase && r.PurchaseLineCount > 0 && Env.Compare(r.ProductUomQty, (double)values["ProductUomQty"], precision) == 1);
            increasedValues = increasedLines.ToDictionary(l => l.Id, l => l.ProductUomQty);
            decreasedValues = decreasedLines.ToDictionary(l => l.Id, l => l.ProductUomQty);
        }

        // Call super.Write(values)
        base.Write(values);

        if (increasedLines.Count > 0) {
            increasedLines.ForEach(line => line.PurchaseIncreaseOrderedQty((double)values["ProductUomQty"], increasedValues));
        }
        if (decreasedLines.Count > 0) {
            decreasedLines.ForEach(line => line.PurchaseDecreaseOrderedQty((double)values["ProductUomQty"], decreasedValues));
        }
    }

    // ... other methods
}
