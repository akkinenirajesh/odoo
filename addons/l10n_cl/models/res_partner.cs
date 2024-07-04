csharp
using System;
using System.Linq;
using System.Collections.Generic;

public partial class ResPartner
{
    public List<string> CommercialFields()
    {
        var baseFields = base.CommercialFields();
        baseFields.Add("L10nClSiiTaxpayerType");
        return baseFields;
    }

    public string FormatVatCl(IDictionary<string, object> values)
    {
        var identificationTypes = new[] {
            Env.Ref("l10n_latam_base.it_vat").Id,
            Env.Ref("l10n_cl.it_RUT").Id,
            Env.Ref("l10n_cl.it_RUN").Id
        };

        var country = Env.Get<Core.Country>().Browse((int)values["CountryId"]);
        var identificationType = Env.Get<L10n_latam.IdentificationType>().Browse((int)values["L10nLatamIdentificationTypeId"]);
        
        bool partnerCountryIsChile = country.Code == "CL" || identificationType.Country.Code == "CL";
        
        if (partnerCountryIsChile &&
            identificationTypes.Contains((int)values["L10nLatamIdentificationTypeId"]) &&
            values.ContainsKey("Vat") &&
            StdNum.Util.GetCcModule("cl", "vat").IsValid((string)values["Vat"]))
        {
            return StdNum.Util.GetCcModule("cl", "vat").Format((string)values["Vat"])
                .Replace(".", "")
                .Replace("CL", "")
                .ToUpper();
        }
        else
        {
            return (string)values["Vat"];
        }
    }

    public string FormatDottedVatCl(string vat)
    {
        var vatParts = vat.Split('-');
        string nVat = vatParts[0], nDv = vatParts[1];
        return $"{int.Parse(nVat):N0}-{nDv}".Replace(",", ".");
    }

    public override void OnCreate(IDictionary<string, object> values)
    {
        if (values.ContainsKey("Vat"))
        {
            values["Vat"] = FormatVatCl(values);
        }
        base.OnCreate(values);
    }

    public override void OnWrite(IDictionary<string, object> values)
    {
        if (values.Keys.Any(k => new[] { "Vat", "L10nLatamIdentificationTypeId", "CountryId" }.Contains(k)))
        {
            var vatValues = new Dictionary<string, object>
            {
                ["Vat"] = values.ContainsKey("Vat") ? values["Vat"] : this.Vat,
                ["L10nLatamIdentificationTypeId"] = values.ContainsKey("L10nLatamIdentificationTypeId") ? values["L10nLatamIdentificationTypeId"] : this.L10nLatamIdentificationType.Id,
                ["CountryId"] = values.ContainsKey("CountryId") ? values["CountryId"] : this.Country.Id
            };
            values["Vat"] = FormatVatCl(vatValues);
        }
        base.OnWrite(values);
    }
}
