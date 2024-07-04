csharp
public partial class AccountTax
{
    public void L10nItEdiCheckExonerationWithNoTax()
    {
        if (this.Country.Code == "IT")
        {
            if (this.AmountType == "percent" && this.Amount == 0 && !(this.L10nItExemptReason != null && !string.IsNullOrEmpty(this.L10nItLawReference)))
            {
                throw new ValidationException("If the tax amount is 0%, you must enter the exoneration code and the related law reference.");
            }
            if (this.L10nItExemptReason == Account.L10nItExemptReason.N6 && this.L10nItIsSplitPayment())
            {
                throw new UserException("Split Payment is not compatible with exoneration of kind 'N6'");
            }
        }
    }

    public string L10nItGetTaxKind()
    {
        if (this.AmountType == "percent" && this.Amount >= 0)
        {
            return "vat";
        }
        return null;
    }

    public bool L10nItIsSplitPayment()
    {
        var taxTags = this.GetTaxTags(isRefund: false, repartitionType: "tax")
            .Union(this.GetTaxTags(isRefund: false, repartitionType: "base"));

        if (!taxTags.Any())
        {
            return false;
        }

        var itTaxReportVe38Lines = Env.Query<Account.AccountReportLine>()
            .Where(l => l.Report.Country.Code == "IT" && l.Code == "VE38")
            .ToList();

        if (!itTaxReportVe38Lines.Any())
        {
            return false;
        }

        var ve38LinesTags = itTaxReportVe38Lines.SelectMany(l => l.ExpressionIds.GetMatchingTags()).ToHashSet();
        return taxTags.Intersect(ve38LinesTags).Any();
    }
}
