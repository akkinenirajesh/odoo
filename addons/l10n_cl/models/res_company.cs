csharp
public partial class ResCompany
{
    public bool LocalizationUseDocuments()
    {
        return this.AccountFiscalCountry.Code == "CL" || base.LocalizationUseDocuments();
    }
}
