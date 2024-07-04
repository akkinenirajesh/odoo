csharp
public partial class l10n_pl_TaxOffice
{
    public string DisplayName { get; set; }

    public void ComputeDisplayName()
    {
        this.DisplayName = $"{this.Code} {this.Name}";
    }
}
