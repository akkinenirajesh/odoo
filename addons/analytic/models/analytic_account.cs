csharp
public partial class AccountAnalyticAccount
{
    public override string ToString()
    {
        string name = Name;
        if (!string.IsNullOrEmpty(Code))
        {
            name = $"[{Code}] {name}";
        }
        if (Partner?.CommercialPartner?.Name != null)
        {
            name = $"{name} - {Partner.CommercialPartner.Name}";
        }
        return name;
    }

    public Dictionary<string, object> CopyData(Dictionary<string, object> defaultValues = null)
    {
        defaultValues = defaultValues ?? new Dictionary<string, object>();
        var result = base.CopyData(defaultValues);
        if (!defaultValues.ContainsKey("Name"))
        {
            result["Name"] = $"{Name} (copy)";
        }
        return result;
    }

    public void CheckCompanyConsistency()
    {
        if (Company != null)
        {
            var env = Env;
            var hasInconsistentLines = env.Set<Account.AccountAnalyticLine>().Search(new[]
            {
                ("AutoAccount", "=", this.Id),
                "!", ("Company", "ChildOf", Company.Id)
            }, limit: 1).Any();

            if (hasInconsistentLines)
            {
                throw new UserError("You can't set a different company on your analytic account since there are some analytic items linked to it.");
            }
        }
    }

    public void ComputeDebitCreditBalance()
    {
        // This method would need to be implemented to calculate the Debit, Credit, and Balance
        // The logic would be similar to the Python version, but adapted for C# and the framework you're using
    }
}
