C#
public partial class ResPartner
{
    public void PeUnlinkExceptMasterData()
    {
        var consumidorFinalAnonimo = Env.Ref("l10n_pe_pos.partner_pe_cf");
        if (consumidorFinalAnonimo == this)
        {
            throw new UserError(
                $"Deleting the partner {consumidorFinalAnonimo.DisplayName} is not allowed because it is required by the Peruvian point of sale."
            );
        }
    }

    public Dictionary<string, object> LoadPosDataFields(int configId)
    {
        var params = base.LoadPosDataFields(configId);
        if (Env.Company.CountryId.Code == "PE")
        {
            params["fields"] += new List<string> { "CityId", "L10nLatamIdentificationTypeId", "L10nPeDistrict" };
        }
        return params;
    }
}
