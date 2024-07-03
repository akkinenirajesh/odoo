csharp
public partial class AccountAnalyticApplicability
{
    public int GetScore(Product.Product product = null, Account.Account account = null)
    {
        int score = 0; // Assume base score calculation is done elsewhere

        if (score == -1)
        {
            return -1;
        }

        if (!string.IsNullOrEmpty(AccountPrefix))
        {
            if (account != null && account.Code.StartsWith(AccountPrefix))
            {
                score += 1;
            }
            else
            {
                return -1;
            }
        }

        if (ProductCateg != null)
        {
            if (product != null && product.Categ == ProductCateg)
            {
                score += 1;
            }
            else
            {
                return -1;
            }
        }

        return score;
    }

    public void ComputeDisplayAccountPrefix()
    {
        DisplayAccountPrefix = BusinessDomain == BusinessDomain.Invoice || 
                               BusinessDomain == BusinessDomain.Bill || 
                               BusinessDomain == BusinessDomain.General;
    }
}
