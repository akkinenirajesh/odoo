csharp
public partial class LunchCashMove
{
    public void ComputeDisplayName()
    {
        this.DisplayName = $"{Env.Translate("Lunch Cashmove")} #{this.Id}";
    }

    public double GetWalletBalance(res.users User, bool IncludeConfig = true)
    {
        var amountSum = Env.SearchRead<LunchCashMove.Report>(new List<object[]> { new object[] { "UserId", "=", User.Id } }, new List<string> { "Amount" }).Sum(x => x.Amount);
        var result = Math.Round(amountSum, 2);
        if (IncludeConfig)
        {
            result += User.CompanyId.LunchMinimumThreshold;
        }
        return result;
    }
}
