csharp
public partial class StockStorageCategory
{
    public void ComputeStorageCapacityIds()
    {
        ProductCapacityIds = CapacityIds.Where(c => c.Product != null).ToList();
        PackageCapacityIds = CapacityIds.Where(c => c.PackageType != null).ToList();
    }

    public void ComputeWeightUomName()
    {
        WeightUomName = Env.Get("product.template")._GetWeightUomNameFromIrConfigParameter();
    }

    public void SetStorageCapacityIds()
    {
        CapacityIds = ProductCapacityIds.Concat(PackageCapacityIds).ToList();
    }

    public List<StockStorageCategory> CopyData()
    {
        return this.Select(category => new StockStorageCategory { Name = $"{category.Name} (copy)" }).ToList();
    }
}
