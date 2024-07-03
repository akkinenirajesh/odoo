csharp
public partial class AccountBankStatement
{
    public string ComputeName()
    {
        return $"{Journal.Code} Statement {Date:d}";
    }

    public void ComputeDateIndex()
    {
        var sortedLines = Lines.OrderBy(l => l.InternalIndex).ToList();
        FirstLineIndex = sortedLines.FirstOrDefault()?.InternalIndex;
        Date = sortedLines.LastOrDefault(l => l.State == "posted")?.Date;
    }

    public void ComputeBalanceStart()
    {
        // Logic to compute balance start
        // This is a simplified version and may need to be adapted
        var previousStatement = Env.AccountBankStatements
            .Where(s => s.FirstLineIndex < FirstLineIndex && s.Journal.Id == Journal.Id)
            .OrderByDescending(s => s.FirstLineIndex)
            .FirstOrDefault();

        BalanceStart = previousStatement?.BalanceEndReal ?? 0m;
    }

    public void ComputeBalanceEnd()
    {
        BalanceEnd = BalanceStart + Lines.Where(l => l.State == "posted").Sum(l => l.Amount);
    }

    public void ComputeBalanceEndReal()
    {
        BalanceEndReal = BalanceEnd;
    }

    public void ComputeCurrency()
    {
        Currency = Journal.Currency ?? Company.Currency;
    }

    public void ComputeJournal()
    {
        Journal = Lines.FirstOrDefault()?.Journal;
    }

    public void ComputeIsComplete()
    {
        IsComplete = Lines.Any(l => l.State == "posted") && 
                     Currency.CompareAmounts(BalanceEnd, BalanceEndReal) == 0;
    }

    public void ComputeIsValid()
    {
        IsValid = GetStatementValidity();
    }

    public void ComputeProblemDescription()
    {
        if (!IsValid)
        {
            ProblemDescription = "The starting balance doesn't match the ending balance of the previous statement, or an earlier statement is missing.";
        }
        else if (!IsComplete)
        {
            ProblemDescription = $"The running balance ({Env.FormatLang(BalanceEnd, Currency)}) doesn't match the specified ending balance.";
        }
        else
        {
            ProblemDescription = null;
        }
    }

    private bool GetStatementValidity()
    {
        var previous = Env.AccountBankStatements
            .Where(s => s.FirstLineIndex < FirstLineIndex && s.Journal.Id == Journal.Id)
            .OrderByDescending(s => s.FirstLineIndex)
            .FirstOrDefault();

        return previous == null || Currency.CompareAmounts(BalanceStart, previous.BalanceEndReal) == 0;
    }
}
