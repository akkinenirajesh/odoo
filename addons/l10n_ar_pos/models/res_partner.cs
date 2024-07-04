csharp
public partial class ResPartner
{
    public void ArUnlinkExceptMasterData()
    {
        var consumidorFinalAnonimo = Env.Ref("l10n_ar.par_cfa").Id;
        if (this.Id == consumidorFinalAnonimo)
        {
            throw new UserError("Deleting this partner is not allowed.");
        }
    }

    public static List<string> LoadPosDataFields(int configId)
    {
        var parameters = base.LoadPosDataFields(configId);
        if (Env.Company.Country.Code == "AR")
        {
            parameters.Add("L10nArAfipResponsibilityTypeId");
            parameters.Add("L10nLatamIdentificationTypeId");
        }
        return parameters;
    }
}
