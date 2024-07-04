csharp
public partial class SaleExpense.Expense {
    public void ComputeCanBeReinvoiced() {
        this.CanBeReinvoiced = Env.Get("Product.Product").Search(p => p.Id == this.ProductId).First().ExpensePolicy in new string[] { "sales_price", "cost" };
    }

    public void ComputeSaleOrderId() {
        if (this.CanBeReinvoiced == false) {
            this.SaleOrderId = 0;
        }
    }

    public void ComputeAnalyticDistribution() {
        var super = Env.Get("SaleExpense.Expense").Search(e => e.Id == this.Id);
        super.ComputeAnalyticDistribution();
        if (this.SaleOrderId != 0) {
            var saleOrder = Env.Get("Sale.SaleOrder").Search(so => so.Id == this.SaleOrderId).First();
            if (saleOrder.AnalyticAccountId != 0) {
                this.AnalyticDistribution = new Dictionary<int, int>() { { saleOrder.AnalyticAccountId, 100 } };
            }
        }
    }

    public void OnchangeSaleOrderId() {
        if (Env.IsProtected("AnalyticDistribution", this) == false) {
            this.InvalidateRecordset("AnalyticDistribution");
            Env.AddToCompute("AnalyticDistribution", this);
        }
    }

    public List<Dictionary<string, object>> GetSplitValues() {
        var vals = Env.Get("SaleExpense.Expense").Search(e => e.Id == this.Id).GetSplitValues();
        foreach (var splitValue in vals) {
            splitValue["SaleOrderId"] = this.SaleOrderId;
        }
        return vals;
    }
}
