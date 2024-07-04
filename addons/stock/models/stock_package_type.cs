C#
public partial class PackageType {
    // all the model methods are written here.

    public void ComputeLengthUomName() {
        this.LengthUomName = Env.Get<ProductTemplate>().GetDefaultLengthUomNameFromIrConfigParameter();
    }

    public void ComputeWeightUomName() {
        this.WeightUomName = Env.Get<ProductTemplate>().GetDefaultWeightUomNameFromIrConfigParameter();
    }

    public List<PackageType> CopyData(Dictionary<string, object> defaultValues = null) {
        List<PackageType> valsList = base.CopyData(defaultValues);
        return valsList.Select(vals => new PackageType {
            Name = string.Format("{0} (copy)", vals.Name)
        }).ToList();
    }
}
