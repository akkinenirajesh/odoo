csharp
public partial class AccountPaymentTerm
{
    public string DefaultExampleDate()
    {
        return Env.Context.ContainsKey("example_date") ? Env.Context["example_date"] : DateTime.Today.ToString("yyyy-MM-dd");
    }

    public void ComputeFiscalCountryCodes()
    {
        var allowedCompanies = Company ?? Env.Companies;
        FiscalCountryCodes = string.Join(",", allowedCompanies.Select(c => c.AccountFiscalCountry?.Code).Where(c => c != null));
    }

    public void ComputeCurrencyId()
    {
        Currency = Company?.Currency ?? Env.Company.Currency;
    }

    public decimal GetAmountDueAfterDiscount(decimal totalAmount, decimal untaxedAmount)
    {
        if (EarlyDiscount)
        {
            var percentage = DiscountPercentage / 100.0m;
            var discountAmountCurrency = EarlyPayDiscountComputation == EarlyPayDiscountComputation.Included
                ? totalAmount * percentage
                : (totalAmount - untaxedAmount) * percentage;
            return Currency.Round(totalAmount - discountAmountCurrency);
        }
        return totalAmount;
    }

    public void ComputeDiscountComputation()
    {
        var countryCode = Company?.CountryCode ?? Env.Company.CountryCode;
        EarlyPayDiscountComputation = countryCode switch
        {
            "BE" => EarlyPayDiscountComputation.Mixed,
            "NL" => EarlyPayDiscountComputation.Excluded,
            _ => EarlyPayDiscountComputation.Included
        };
    }

    public void ComputeExampleInvalid()
    {
        ExampleInvalid = !LineIds.Any();
    }

    public void ComputeExamplePreview()
    {
        // Implementation for ComputeExamplePreview
        // This would involve complex logic to generate the HTML preview
        // and set ExamplePreview and ExamplePreviewDiscount properties
    }

    public override string ToString()
    {
        return Name;
    }
}
