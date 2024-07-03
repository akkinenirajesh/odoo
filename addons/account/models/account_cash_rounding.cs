csharp
public partial class AccountCashRounding
{
    public override string ToString()
    {
        return Name;
    }

    public void ValidateRounding()
    {
        if (Rounding <= 0)
        {
            throw new ValidationException("Please set a strictly positive rounding value.");
        }
    }

    public decimal Round(decimal amount)
    {
        return Math.Round(amount, (int)Math.Log10(1 / Rounding), (MidpointRounding)RoundingMethod);
    }

    public decimal ComputeDifference(Core.Currency currency, decimal amount)
    {
        amount = currency.Round(amount);
        decimal difference = Round(amount) - amount;
        return currency.Round(difference);
    }
}
