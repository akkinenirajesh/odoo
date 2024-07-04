csharp
public partial class ResCompany
{
    public bool LocalizationUseDocuments { get; set; }

    public void ComputeLocalizationUseDocuments()
    {
        this.LocalizationUseDocuments = Env.GetContext().Company.AccountFiscalCountryId.Code == "PE" || base.ComputeLocalizationUseDocuments();
    }
}
