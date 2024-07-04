csharp
public partial class AccountAnalyticApplicability
{
    public bool ComputeDisplayAccountPrefix()
    {
        // Logic to compute display account prefix
        if (BusinessDomain == BusinessDomain.Expense)
        {
            return true;
        }
        // Add logic for other cases if needed
        return false;
    }
}

public partial class AccountAnalyticAccount
{
    public void UnlinkExceptAccountInAnalyticDistribution()
    {
        var expenseIds = Env.Cr.Query<int>(
            @"SELECT id FROM hr_expense
              WHERE @p0 && analytic_distribution
              LIMIT 1",
            new object[] { Id }
        );

        if (expenseIds.Any())
        {
            throw new UserError("You cannot delete an analytic account that is used in an expense.");
        }
    }
}
