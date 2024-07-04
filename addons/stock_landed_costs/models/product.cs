csharp
public partial class Stock.ProductTemplate {
    public void Write(Dictionary<string, object> vals) {
        foreach (Stock.ProductTemplate product in this.Env.GetRecords<Stock.ProductTemplate>(this.Id)) {
            if (((vals.ContainsKey("Type") && vals["Type"] != "service") || (vals.ContainsKey("LandedCostOk") && !(bool)vals["LandedCostOk"])) && product.Type == "service" && product.LandedCostOk) {
                if (this.Env.GetRecords<Account.AccountMoveLine>().Where(r => r.ProductId.Contains(product.ProductVariantIds)).Count(r => r.IsLandedCostsLine) > 0) {
                    throw new Exception("You cannot change the product type or disable landed cost option because the product is used in an account move line.");
                }
                vals["LandedCostOk"] = false;
            }
        }
        this.Env.Write(this.Id, vals);
    }
}
