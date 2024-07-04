csharp
public partial class ResCompany
{
    public void OnChangeCountry()
    {
        if (Country?.Code == "AR")
        {
            TaxCalculationRoundingMethod = "round_globally";
        }
    }

    public void ComputeL10nArCompanyRequiresVat()
    {
        L10nArCompanyRequiresVat = L10nArAfipResponsibilityType?.Code == "1";
    }

    public bool LocalizationUseDocuments()
    {
        return Env.Get<Core.Country>(AccountFiscalCountry)?.Code == "AR" || base.LocalizationUseDocuments();
    }

    public override void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("L10nArAfipResponsibilityType"))
        {
            var newAfipResponsibilityTypeId = (int)vals["L10nArAfipResponsibilityType"];
            if (newAfipResponsibilityTypeId != L10nArAfipResponsibilityType?.Id && ExistingAccounting())
            {
                throw new UserError("Could not change the AFIP Responsibility of this company because there are already accounting entries.");
            }
        }

        base.Write(vals);
    }

    private bool ExistingAccounting()
    {
        // Implement the logic to check for existing accounting entries
        return false;
    }
}
