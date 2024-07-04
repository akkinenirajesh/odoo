C#
public partial class ResCompany {
    public bool LocalizationUseDocuments() {
        return Env.Company.AccountFiscalCountryId.Code == "UY" || base.LocalizationUseDocuments();
    }
}
