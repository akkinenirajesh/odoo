csharp
public partial class PointOfSalePosCombo
{
    public virtual int NumberOfProducts { get; set; }
    public virtual float BasePrice { get; set; }

    public void ComputeNumberOfProducts()
    {
        this.NumberOfProducts = 0;

        // optimization trick to count the number of products in each combo
        var comboLineCount = Env.Model<PointOfSalePosComboLine>().ReadGroup(
            new[] { ("ComboId", this.Id) },
            new[] { "ComboId" },
            new[] { "__count" });
        this.NumberOfProducts = comboLineCount.FirstOrDefault()?.Count ?? 0;
    }

    public void ComputeBasePrice()
    {
        if (this.ComboLineIds == null)
        {
            this.BasePrice = 0;
            return;
        }

        // Use the lowest price of the combo lines as the base price
        this.BasePrice = this.ComboLineIds.Select(l => Env.Model<ProductProduct>().Get(l.ProductId).LstPrice).Min();
    }

    public void CheckComboLineIdsIsNotNull()
    {
        if (this.ComboLineIds == null)
        {
            throw new ValidationException("Please add products in combo.");
        }
    }

    public virtual List<int> _LoadPosDataDomain(List<Dictionary<string, object>> data)
    {
        var comboIds = data.Where(p => p.ContainsKey("combo_ids")).SelectMany(p => (List<int>)p["combo_ids"]).ToList();
        return comboIds;
    }

    public virtual List<string> _LoadPosDataFields(int configId)
    {
        return new List<string> { "Id", "Name", "ComboLineIds", "BasePrice" };
    }
}
