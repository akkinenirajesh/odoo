csharp
public partial class MrpAccount.StockRule
{
    public virtual MrpAccount.StockRule PrepareMoVals(Product.Product productID, double productQty, Product.Uom productUomID, Stock.Location locationID, string name, string origin, Core.Company companyID, Dictionary<string, object> values, Mrp.Bom bom)
    {
        var res = Env.Call("stock.rule", "_prepare_mo_vals", productID, productQty, productUomID, locationID, name, origin, companyID, values, bom);
        if (bom.AnalyticDistribution == null)
        {
            if (values.ContainsKey("AnalyticDistribution"))
            {
                res["AnalyticDistribution"] = values["AnalyticDistribution"];
            }
            else if (values.ContainsKey("AnalyticAccountID"))
            {
                res["AnalyticDistribution"] = new Dictionary<long, double>() { { values["AnalyticAccountID"].GetLongValue(), 100 } };
            }
        }
        return res as MrpAccount.StockRule;
    }
}
