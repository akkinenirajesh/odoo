csharp
public partial class StockQuantPackage {
    public StockQuantPackage(BuviContext env) {
        Env = env;
    }

    private BuviContext Env { get; set; }

    public void ComputeWeight() {
        if (Env.Context.ContainsKey("picking_id")) {
            var packageWeights = new Dictionary<long, double>();
            var resGroups = Env.Model<StockMoveLine>().ReadGroup(
                new List<object>() { new { result_package_id = this.Id }, new { product_id = new object[] { "!=" , null } }, new { picking_id = Env.Context["picking_id"] } },
                new List<object>() { "result_package_id", "product_id", "product_uom_id", "quantity" },
                new List<object>() { "__count" }
            );
            foreach (var resGroup in resGroups) {
                var resultPackage = resGroup[0];
                var product = resGroup[1];
                var productUom = resGroup[2];
                var quantity = resGroup[3];
                var count = resGroup[4];
                packageWeights[Convert.ToInt64(resultPackage)] += count * productUom.ComputeQuantity(quantity, product.UomId) * product.Weight;
            }
            this.Weight = this.PackageTypeId.BaseWeight + packageWeights[this.Id];
        } else {
            var weight = this.PackageTypeId.BaseWeight;
            foreach (var quant in this.QuantIds) {
                weight += quant.Quantity * quant.ProductId.Weight;
            }
            this.Weight = weight;
        }
    }

    public string GetDefaultWeightUom() {
        return Env.Model<ProductTemplate>().GetWeightUomNameFromIrConfigParameter();
    }

    public void ComputeWeightUomName() {
        this.WeightUomName = Env.Model<ProductTemplate>().GetWeightUomNameFromIrConfigParameter();
    }

    public void ComputeWeightIsKg() {
        this.WeightIsKg = false;
        var uomId = Env.Model<ProductTemplate>().GetWeightUomIdFromIrConfigParameter();
        if (uomId == Env.Ref("uom.product_uom_kgm")) {
            this.WeightIsKg = true;
        }
        this.WeightUomRounding = uomId.Rounding;
    }
}
