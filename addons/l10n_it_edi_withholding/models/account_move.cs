csharp
public partial class AccountMove
{
    public void ComputeAmountExtended()
    {
        var totals = new Dictionary<string, decimal>
        {
            { "vat", 0m },
            { "withholding", 0m },
            { "pension_fund", 0m }
        };

        if (IsInvoice(true))
        {
            foreach (var line in LineIds.Where(l => l.TaxLineId != null))
            {
                var kind = line.TaxLineId.L10nItGetTaxKind();
                if (totals.ContainsKey(kind))
                {
                    totals[kind] -= line.Balance;
                }
            }
        }

        L10nItAmountVatSigned = totals["vat"];
        L10nItAmountWithholdingSigned = totals["withholding"];
        L10nItAmountPensionFundSigned = totals["pension_fund"];
        L10nItAmountBeforeWithholdingSigned = AmountUntaxedSigned + totals["vat"] + totals["pension_fund"];
    }

    public bool L10nItEdiFilterTaxDetails(AccountMoveLine line, Dictionary<string, object> taxValues)
    {
        var repartitionLine = (AccountTaxRepartitionLine)taxValues["tax_repartition_line"];
        var repartitionLineVat = repartitionLine.TaxId.L10nItFilterKind("vat");
        return repartitionLine.FactorPercent >= 0 && repartitionLineVat != null && repartitionLineVat.Amount >= 0;
    }

    public Dictionary<string, object> L10nItEdiGetValues(Dictionary<string, object> pdfValues = null)
    {
        var templateValues = base.L10nItEdiGetValues(pdfValues);

        // Withholding tax data
        var withholdingLines = LineIds.Where(x => x.TaxLineId.L10nItFilterKind("withholding") != null);
        var withholdingValues = withholdingLines.Select(x => new { Tax = x.TaxLineId, TaxAmount = Math.Abs(x.Balance) }).ToList();

        // Fix the total as it must be computed before applying the Withholding
        var documentTotal = (decimal)templateValues["document_total"];
        documentTotal -= L10nItAmountWithholdingSigned;

        // Pension fund tax data
        var pensionFundLines = LineIds.Where(line => line.TaxLineId.L10nItFilterKind("pension_fund") != null);
        var pensionFundMapping = new Dictionary<int, Tuple<AccountTax, AccountTax>>();
        foreach (var line in LineIds)
        {
            var pensionFundTax = line.TaxIds.L10nItFilterKind("pension_fund");
            if (pensionFundTax != null)
            {
                pensionFundMapping[pensionFundTax.Id] = new Tuple<AccountTax, AccountTax>(
                    line.TaxIds.L10nItFilterKind("vat"),
                    line.TaxIds.L10nItFilterKind("withholding")
                );
            }
        }

        var pensionFundValues = new List<object>();
        var enasarcoTaxes = new List<AccountTax>();
        foreach (var line in pensionFundLines)
        {
            if (line.TaxLineId.L10nItPensionFundType == "TC07")
            {
                enasarcoTaxes.Add(line.TaxLineId);
                continue;
            }
            var pensionFundTax = line.TaxLineId;
            var (vatTax, withholdingTax) = pensionFundMapping[pensionFundTax.Id];
            pensionFundValues.Add(new
            {
                Tax = pensionFundTax,
                BaseAmount = line.TaxBaseAmount,
                TaxAmount = Math.Abs(line.Balance),
                VatTax = vatTax,
                WithholdingTax = withholdingTax
            });
        }

        // Enasarco pension fund
        Dictionary<int, object> enasarcoValues = null;
        if (enasarcoTaxes.Any())
        {
            enasarcoValues = new Dictionary<int, object>();
            var enasarcoDetails = PrepareInvoiceAggregatedTaxes(
                (line, taxValues) => Env.Get<AccountTax>().Browse(new[] { (int)taxValues["id"] }).L10nItPensionFundType == "TC07"
            );
            foreach (var detail in enasarcoDetails["tax_details_per_record"].Values)
            {
                foreach (var subdetail in ((Dictionary<string, object>)detail["tax_details"]).Values)
                {
                    documentTotal += Math.Abs((decimal)subdetail["tax_amount"]);
                    var line = ((List<AccountMoveLine>)subdetail["records"]).First();
                    enasarcoValues[line.Id] = new
                    {
                        Amount = ((AccountTax)subdetail["tax"]).Amount,
                        TaxAmount = Math.Abs((decimal)subdetail["tax_amount"])
                    };
                }
            }
        }

        // Update the template_values
        templateValues["withholding_values"] = withholdingValues;
        templateValues["pension_fund_values"] = pensionFundValues;
        templateValues["enasarco_values"] = enasarcoValues;
        templateValues["document_total"] = documentTotal;

        return templateValues;
    }

    public List<string> L10nItEdiExportTaxesDataCheck()
    {
        var errors = new List<string>();
        foreach (var invoiceLine in InvoiceLineIds.Where(x => x.DisplayType == "product"))
        {
            var allTaxes = invoiceLine.TaxIds.FlattenTaxesHierarchy();
            var vatTaxes = allTaxes.L10nItFilterKind("vat");
            var withholdingTaxes = allTaxes.L10nItFilterKind("withholding");
            var pensionFundTaxes = allTaxes.L10nItFilterKind("pension_fund");

            if (vatTaxes.Where(x => x.Amount >= 0).Count() != 1)
            {
                errors.Add($"Bad tax configuration for line {invoiceLine.Name}, there must be one and only one VAT tax per line");
            }
            if (pensionFundTaxes.Count() > 1 || withholdingTaxes.Count() > 1)
            {
                errors.Add($"Bad tax configuration for line {invoiceLine.Name}, there must be one Withholding tax and one Pension Fund tax at max.");
            }
        }
        return errors;
    }
}
