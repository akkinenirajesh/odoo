csharp
public partial class AccountMove
{
    public string GetNameInvoiceReport()
    {
        return this.Country?.Code == "HU" ? "l10n_hu_edi.report_invoice_document" : base.GetNameInvoiceReport();
    }

    public Dictionary<string, object> GetL10nHuInvoiceTotalsForReport()
    {
        var taxTotals = this.TaxTotals;
        if (taxTotals == null)
        {
            return null;
        }

        taxTotals["display_tax_base"] = true;

        if (this.MoveType.Contains("refund"))
        {
            InvertDict(taxTotals, new[] { "amount_total", "amount_untaxed", "rounding_amount", "amount_total_rounded" });

            foreach (var subtotal in (List<Dictionary<string, object>>)taxTotals["subtotals"])
            {
                InvertDict(subtotal, new[] { "amount" });
            }

            foreach (var taxList in ((Dictionary<string, List<Dictionary<string, object>>>)taxTotals["groups_by_subtotal"]).Values)
            {
                foreach (var tax in taxList)
                {
                    var keysToInvert = new[] { "tax_group_amount", "tax_group_base_amount", "tax_group_amount_company_currency", "tax_group_base_amount_company_currency" };
                    InvertDict(tax, keysToInvert);
                }
            }
        }

        var currencyHuf = Env.Ref("base.HUF");
        var currencyRate = GetL10nHuCurrencyRate();

        taxTotals["total_vat_amount_in_huf"] = this.LineIds
            .Where(l => l.TaxLineId?.L10nHuTaxType != null)
            .Sum(l => this.Company.Currency == currencyHuf ? -l.Balance : currencyHuf.Round(-l.AmountCurrency * currencyRate));

        taxTotals["formatted_total_vat_amount_in_huf"] = Env.FormatLang(
            taxTotals["total_vat_amount_in_huf"], currencyObj: currencyHuf
        );

        return taxTotals;
    }

    private void InvertDict(Dictionary<string, object> dictionary, string[] keysToInvert)
    {
        foreach (var key in keysToInvert)
        {
            if (dictionary.ContainsKey(key) && dictionary[key] is decimal value)
            {
                dictionary[key] = -value;
            }
        }

        var keysToReformat = keysToInvert.ToDictionary(k => $"formatted_{k}", k => k);
        foreach (var kvp in keysToReformat)
        {
            if (dictionary.ContainsKey(kvp.Value))
            {
                dictionary[kvp.Key] = Env.FormatLang(dictionary[kvp.Value], currencyObj: this.Company.Currency);
            }
        }
    }

    // Other methods would be implemented here...
}
