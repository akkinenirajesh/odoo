csharp
public partial class AccountTax
{
    public string L10nItGetTaxKind()
    {
        if (!string.IsNullOrEmpty(L10nItWithholdingType))
            return "withholding";
        if (!string.IsNullOrEmpty(L10nItPensionFundType))
            return "pension_fund";
        return base.L10nItGetTaxKind(); // Assuming there's a base implementation
    }

    public void ValidateWithholding()
    {
        if (!string.IsNullOrEmpty(L10nItWithholdingType) && L10nItWithholdingType != "RT04" && Amount >= 0)
        {
            throw new ValidationException($"Tax '{Name}' has a withholding type so the amount must be negative.");
        }

        if (!string.IsNullOrEmpty(L10nItWithholdingType) && string.IsNullOrEmpty(L10nItWithholdingReason))
        {
            throw new ValidationException($"Tax '{Name}' has a withholding type, so the withholding reason must also be specified");
        }

        if (!string.IsNullOrEmpty(L10nItWithholdingReason) && string.IsNullOrEmpty(L10nItWithholdingType))
        {
            throw new ValidationException($"Tax '{Name}' has a withholding reason, so the withholding type must also be specified");
        }

        if ((!string.IsNullOrEmpty(L10nItWithholdingType) || !string.IsNullOrEmpty(L10nItWithholdingReason)) && !string.IsNullOrEmpty(L10nItPensionFundType))
        {
            throw new ValidationException($"Tax '{Name}' cannot be both a Withholding tax and a Pension fund tax. Please create two separate ones.");
        }
    }
}
