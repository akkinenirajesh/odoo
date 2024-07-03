csharp
public partial class ReconcileModel
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<Company> GetEligibleCompanies()
    {
        return Env.Companies.Where(c => c.ChartTemplate != null);
    }
}

public partial class ReconcileModelLine
{
    public string GetAmount()
    {
        switch (AmountType)
        {
            case ReconcileModelLineAmountType.Fixed:
                return Env.Random.Next(1, 1001).ToString();
            case ReconcileModelLineAmountType.Percentage:
                return Env.Random.Next(1, 101).ToString();
            case ReconcileModelLineAmountType.Regex:
                return Env.Random.Choice(new[] { @"^invoice \d+ (\d+)$", @"xd no-(\d+)" });
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Account GetRandomAccount()
    {
        var companyId = Model.Company.Id;
        var accounts = Env.Accounts.Search(a => a.Company.Id == companyId);
        return Env.Random.Choice(accounts);
    }
}
