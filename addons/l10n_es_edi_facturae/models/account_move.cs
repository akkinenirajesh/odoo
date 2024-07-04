csharp
public partial class AccountMove
{
    public override string ToString()
    {
        return Name;
    }

    public bool L10nEsEdiFacturaeGetDefaultEnable()
    {
        return !InvoicePdfReportId.HasValue
            && !L10nEsEdiFacturaeXmlId.HasValue
            && !L10nEsIsSimplified
            && IsInvoice(includeReceipts: true)
            && CompanyId.CountryCode == "ES"
            && CompanyId.CurrencyId.Name == "EUR";
    }

    public string L10nEsEdiFacturaeGetFilename()
    {
        return $"{Name.Replace("/", "_")}_facturae_signed.xml";
    }

    public Dictionary<string, DateTime> L10nEsEdiFacturaeGetTaxPeriod()
    {
        DateTime periodStart, periodEnd;

        if (Env.Company.Fields.ContainsKey("AccountTaxPeriodicity"))
        {
            (periodStart, periodEnd) = Env.Company.GetTaxClosingPeriodBoundaries(Date);
        }
        else
        {
            periodStart = DateUtils.StartOf(Date, "month");
            periodEnd = DateUtils.EndOf(Date, "month");
        }

        return new Dictionary<string, DateTime> { { "start", periodStart }, { "end", periodEnd } };
    }

    // Other methods would be implemented similarly, adapting Odoo's Python logic to C#
    // and using the appropriate C# constructs and Buvi framework utilities.
}
