csharp
public partial class Sale_SaleOrderLine {
    public void ComputePurchasePrice() {
        var timesheetSols = this.Where(sol => sol.QtyDeliveredMethod == "timesheet" && !sol.ProductId.StandardPrice).ToList();
        var remainingSols = this.Except(timesheetSols).ToList();
        Env.Call("Sale.SaleOrderLine", "_ComputePurchasePrice", remainingSols);
        if (timesheetSols.Count > 0) {
            var groupAmount = Env.Call("Account.AnalyticLine", "_ReadGroup", new object[] {
                new List<object> { 
                    new Dictionary<string, object> { { "SoLine", timesheetSols.Select(sol => sol.Id).ToList() }, { "ProjectId", new object[] { "!=", false } } }
                }, 
                new List<string> { "SoLine" },
                new List<string> { "Amount:sum", "UnitAmount:sum" }
            });
            var mappedSolTimesheetAmount = groupAmount.ToDictionary(item => item["SoLine"].ToString(), item => (decimal)item["Amount:sum"] / (decimal)item["UnitAmount:sum"]);
            foreach (var line in timesheetSols) {
                line = line.WithCompany(line.Company);
                var productCost = mappedSolTimesheetAmount.ContainsKey(line.Id.ToString()) ? (decimal)mappedSolTimesheetAmount[line.Id.ToString()] : line.ProductId.StandardPrice;
                var productUom = line.ProductUom ?? line.ProductId.Uom;
                if (productUom != line.Company.ProjectTimeMode && productUom.Category.Id == line.Company.ProjectTimeMode.Category.Id) {
                    productCost = productUom.ComputeQuantity(productCost, line.Company.ProjectTimeMode);
                }
                line.PurchasePrice = line.ConvertToSolCurrency(productCost, line.ProductId.CostCurrency);
            }
        }
    }

    public decimal ConvertToSolCurrency(decimal amount, Res_Currency currency) {
        return amount;
    }
}
