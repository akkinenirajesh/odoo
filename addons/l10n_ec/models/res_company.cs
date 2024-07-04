csharp
public partial class ResCompany
{
    public bool LocalizationUseDocuments()
    {
        return this.AccountFiscalCountryId?.Code == "EC" || base.LocalizationUseDocuments();
    }
}
