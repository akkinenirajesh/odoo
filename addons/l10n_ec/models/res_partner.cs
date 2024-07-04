csharp
public partial class ResPartner
{
    public string ComputeL10nEcVatValidation()
    {
        var itRuc = Env.Ref("l10n_ec.ec_ruc");
        var itDni = Env.Ref("l10n_ec.ec_dni");
        var ruc = StdNum.Util.GetCcModule("ec", "ruc");
        var ci = StdNum.Util.GetCcModule("ec", "ci");

        if (L10nLatamIdentificationType == itRuc || L10nLatamIdentificationType == itDni)
        {
            if (!string.IsNullOrEmpty(Vat))
            {
                bool finalConsumer = VerifyFinalConsumer(Vat);
                if (!finalConsumer)
                {
                    if (L10nLatamIdentificationType == itDni && !ci.IsValid(Vat))
                    {
                        return $"The VAT {Vat} seems to be invalid as the tenth digit doesn't comply with the validation algorithm (could be an old VAT number)";
                    }
                    if (L10nLatamIdentificationType == itRuc && !ruc.IsValid(Vat))
                    {
                        return $"The VAT {Vat} seems to be invalid as the tenth digit doesn't comply with the validation algorithm (SRI has stated that this validation is not required anymore for some VAT numbers)";
                    }
                }
            }
        }
        return null;
    }

    public void CheckVat()
    {
        var itRuc = Env.Ref("l10n_ec.ec_ruc");
        var itDni = Env.Ref("l10n_ec.ec_dni");
        var ecuadorCountry = Env.Ref("base.ec");

        if (Country == ecuadorCountry && !string.IsNullOrEmpty(Vat))
        {
            if (L10nLatamIdentificationType == itDni || L10nLatamIdentificationType == itRuc)
            {
                if (L10nLatamIdentificationType == itDni && Vat.Length != 10)
                {
                    throw new ValidationException($"If your identification type is {itDni.DisplayName}, it must be 10 digits");
                }
                if (L10nLatamIdentificationType == itRuc && Vat.Length != 13)
                {
                    throw new ValidationException($"If your identification type is {itRuc.DisplayName}, it must be 13 digits");
                }
            }
        }
    }

    public string L10nEcGetIdentificationType()
    {
        var idTypesByXmlid = new Dictionary<string, string>
        {
            {"l10n_ec.ec_dni", "cedula"},
            {"l10n_ec.ec_ruc", "ruc"},
            {"l10n_ec.ec_passport", "ec_passport"},
            {"l10n_latam_base.it_pass", "passport"},
            {"l10n_latam_base.it_fid", "foreign"},
            {"l10n_latam_base.it_vat", "foreign"}
        };

        var xmlidByResId = Env.GetXmlidByResId(idTypesByXmlid.Keys.ToList());

        if (xmlidByResId.TryGetValue(L10nLatamIdentificationType.Id, out string idTypeXmlid))
        {
            if (idTypesByXmlid.TryGetValue(idTypeXmlid, out string idType))
            {
                return idType;
            }
        }

        if (L10nLatamIdentificationType.Country.Code != "EC")
        {
            return "foreign";
        }

        return null;
    }

    private bool VerifyFinalConsumer(string vat)
    {
        return vat == new string('9', 13);
    }
}
