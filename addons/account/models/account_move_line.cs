csharp
public partial class AccountMoveLine
{
    public override string ToString()
    {
        return $"{MoveName} - {Name}";
    }

    public void OpenReconcileView()
    {
        // Implementation for opening reconcile view
    }

    public void ActionOpenBusinessDoc()
    {
        Move.ActionOpenBusinessDoc();
    }

    public void ActionAutomaticEntry(string defaultAction = null)
    {
        // Implementation for automatic entry action
    }

    public void ActionAddFromCatalog()
    {
        Move.ActionAddFromCatalog();
    }

    public Dictionary<string, object> GetProductCatalogLinesData()
    {
        if (this.Any())
        {
            Product.EnsureOne();
            var result = Move.GetProductPriceAndData(Product);
            result["quantity"] = this.Sum(line => 
                line.ProductUom.ComputeQuantity(line.Quantity, line.Product.Uom));
            result["readOnly"] = Move.IsReadonly() || this.Count() > 1;
            return result;
        }
        return new Dictionary<string, object> { { "quantity", 0 } };
    }

    public void ConditionalAddToCompute(string fieldName, Func<AccountMoveLine, bool> condition)
    {
        // Implementation for conditional add to compute
    }

    public List<AccountMoveLine> GetDownpaymentLines()
    {
        return new List<AccountMoveLine>();
    }
}
