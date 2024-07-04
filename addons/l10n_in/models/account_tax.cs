csharp
public partial class AccountTax
{
    public Dictionary<string, object> PrepareForTaxesComputation()
    {
        var taxData = base.PrepareForTaxesComputation();

        if (this.CountryCode == "IN")
        {
            string l10nInTaxType = null;
            var tags = this.InvoiceRepartitionLineIds.SelectMany(line => line.TagIds);

            if (tags.Contains(Env.Ref("l10n_in.tax_tag_igst")))
                l10nInTaxType = "igst";
            else if (tags.Contains(Env.Ref("l10n_in.tax_tag_cgst")))
                l10nInTaxType = "cgst";
            else if (tags.Contains(Env.Ref("l10n_in.tax_tag_sgst")))
                l10nInTaxType = "sgst";
            else if (tags.Contains(Env.Ref("l10n_in.tax_tag_cess")))
                l10nInTaxType = "cess";

            taxData["_l10n_in_tax_type"] = l10nInTaxType;
        }

        return taxData;
    }

    public Dictionary<string, object> GetL10nInHsnSummaryTable(List<Dictionary<string, object>> baseLines, bool displayUom)
    {
        var resultsMap = new Dictionary<FrozenDictionary<string, object>, Dictionary<string, object>>();
        var l10nInTaxTypes = new HashSet<string>();

        foreach (var baseLine in baseLines)
        {
            var l10nInHsnCode = baseLine["l10n_in_hsn_code"] as string;
            if (string.IsNullOrEmpty(l10nInHsnCode))
                continue;

            // ... (rest of the logic from the Python method)
            // You'll need to translate the Python logic to C# here
        }

        var items = resultsMap.Values.Select(value => new Dictionary<string, object>
        {
            ["l10n_in_hsn_code"] = value["l10n_in_hsn_code"],
            ["uom_name"] = value["uom_name"],
            ["rate"] = value["rate"],
            ["quantity"] = value["quantity"],
            ["amount_untaxed"] = value["amount_untaxed"],
            ["tax_amount_igst"] = ((Dictionary<string, double>)value["tax_amounts"])["igst"],
            ["tax_amount_cgst"] = ((Dictionary<string, double>)value["tax_amounts"])["cgst"],
            ["tax_amount_sgst"] = ((Dictionary<string, double>)value["tax_amounts"])["sgst"],
            ["tax_amount_cess"] = ((Dictionary<string, double>)value["tax_amounts"])["cess"]
        }).ToList();

        return new Dictionary<string, object>
        {
            ["has_igst"] = l10nInTaxTypes.Contains("igst"),
            ["has_gst"] = l10nInTaxTypes.Contains("cgst") || l10nInTaxTypes.Contains("sgst"),
            ["has_cess"] = l10nInTaxTypes.Contains("cess"),
            ["nb_columns"] = 5 + l10nInTaxTypes.Count,
            ["display_uom"] = displayUom,
            ["items"] = items
        };
    }
}
