csharp
public partial class Account
{
    public override string ToString()
    {
        return $"{Code} {Name}";
    }

    public void ComputeAccountRoot()
    {
        if (string.IsNullOrEmpty(Code))
        {
            RootId = null;
        }
        else
        {
            int rootId = (Code[0] * 1000) + (Code.Length > 1 ? Code[1] : 0);
            RootId = Env.Ref<AccountRoot>(rootId);
        }
    }

    public void ComputeAccountGroup()
    {
        if (string.IsNullOrEmpty(Code))
        {
            GroupId = null;
            return;
        }

        var groups = Env.Set<AccountGroup>()
            .Where(g => g.CompanyId == CompanyId && 
                        g.CodePrefixStart <= Code.Substring(0, g.CodePrefixStart.Length) &&
                        g.CodePrefixEnd >= Code.Substring(0, g.CodePrefixEnd.Length))
            .OrderByDescending(g => g.CodePrefixStart.Length)
            .ToList();

        GroupId = groups.FirstOrDefault();
    }

    public void ComputeCurrentBalance()
    {
        var balance = Env.Set<AccountMoveLine>()
            .Where(l => l.AccountId == this && l.ParentState == "posted")
            .Sum(l => l.Balance);

        CurrentBalance = balance;
    }

    public void ComputeRelatedTaxesAmount()
    {
        var count = Env.Set<AccountTax>()
            .Where(t => t.RepartitionLines.Any(l => l.AccountId == this))
            .Count();

        RelatedTaxesAmount = count;
    }

    public void ComputeInternalGroup()
    {
        if (AccountType != null)
        {
            InternalGroup = (Account.InternalGroup)Enum.Parse(typeof(Account.InternalGroup), AccountType.ToString().Split('_')[0], true);
        }
    }

    public void ComputeOpeningDebitCredit()
    {
        var openingMove = CompanyId.AccountOpeningMoveId;
        if (openingMove != null)
        {
            var lines = Env.Set<AccountMoveLine>()
                .Where(l => l.MoveId == openingMove && l.AccountId == this);

            OpeningDebit = lines.Sum(l => l.Debit);
            OpeningCredit = lines.Sum(l => l.Credit);
            OpeningBalance = OpeningDebit - OpeningCredit;
        }
        else
        {
            OpeningDebit = 0;
            OpeningCredit = 0;
            OpeningBalance = 0;
        }
    }
}
