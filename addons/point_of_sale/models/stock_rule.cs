C#
public partial class StockRule {
    public virtual StockMove _GetStockMoveValues(Product.Product ProductId, double ProductQty, Product.Uom ProductUomId, Stock.Location LocationId, string Name, string Origin, Company.Company CompanyId, Dictionary<string, object> Values) {
        StockMove moveValues = Env.Call<StockMove>("_GetStockMoveValues", this, ProductId, ProductQty, ProductUomId, LocationId, Name, Origin, CompanyId, Values);
        if (Values.ContainsKey("ProductDescriptionVariants") && Values.ContainsKey("GroupId") && (Values["GroupId"] as Stock.ProcurementGroup).PosOrderId != null) {
            moveValues.DescriptionPicking = Values["ProductDescriptionVariants"] as string;
        }
        return moveValues;
    }
}
