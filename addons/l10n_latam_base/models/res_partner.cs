csharp
public partial class ResPartner
{
    public override IEnumerable<string> CommercialFields()
    {
        var fields = base.CommercialFields().ToList();
        fields.Add(nameof(L10nLatamIdentificationType));
        return fields;
    }

    public void CheckVat()
    {
        if (L10nLatamIdentificationType?.IsVat == true)
        {
            // Implement the VAT check logic here
            // You might need to call a base implementation or use a different approach
            // since we don't have direct access to the Odoo super() call
        }
    }

    public void OnChangeCountry()
    {
        var country = Country ?? Company?.AccountFiscalCountry ?? Env.Company.AccountFiscalCountry;
        var identificationType = L10nLatamIdentificationType;

        if (identificationType == null || identificationType.Country != country)
        {
            L10nLatamIdentificationType = Env.Set<L10nLatam.IdentificationType>()
                .Search(x => x.Country == country && x.IsVat)
                .FirstOrDefault() ?? Env.Ref<L10nLatam.IdentificationType>("l10n_latam_base.it_vat");
        }
    }
}
