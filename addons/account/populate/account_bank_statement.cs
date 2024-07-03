csharp
public partial class BankStatement
{
    public override string ToString()
    {
        return Name;
    }

    public void Populate(int size)
    {
        var rand = new Random(Env.GetSeed("account_bank_statement+Populate"));

        var readGroupRes = Env.BankStatementLines
            .Where(l => l.Statement == null)
            .GroupBy(l => l.Journal)
            .Select(g => new { Journal = g.Key, Ids = g.Select(l => l.Id).ToList() })
            .ToList();

        var bankStatementValsList = new List<BankStatement>();
        foreach (var group in readGroupRes)
        {
            int nbIds = group.Ids.Count;
            while (nbIds > 0)
            {
                int batchSize = Math.Min(rand.Next(1, 20), nbIds);
                nbIds -= batchSize;

                // 50% chance to create a statement
                bool statementNeeded = rand.Next(2) == 0;
                if (!statementNeeded)
                    continue;

                var newStatement = new BankStatement
                {
                    Name = $"statement_{bankStatementValsList.Count + 1}",
                    Journal = group.Journal,
                    Lines = Env.BankStatementLines.Where(l => group.Ids.Contains(l.Id)).ToList()
                };
                bankStatementValsList.Add(newStatement);
            }
        }

        Env.BankStatements.AddRange(bankStatementValsList);
    }
}

public partial class BankStatementLine
{
    public override string ToString()
    {
        return $"{Date} - {Amount}";
    }

    private static Dictionary<int, List<int>> _partnerCache = new Dictionary<int, List<int>>();

    private List<int> SearchPartnerIds(int companyId)
    {
        if (!_partnerCache.ContainsKey(companyId))
        {
            _partnerCache[companyId] = Env.Partners
                .Where(p => p.Company.Id == companyId)
                .Select(p => p.Id)
                .ToList();
        }
        return _partnerCache[companyId];
    }

    private Core.Partner GetPartner(Random random, int journalId)
    {
        var companyId = Env.Journals.First(j => j.Id == journalId).Company.Id;
        var partnerIds = SearchPartnerIds(companyId);
        if (random.Next(partnerIds.Count + 1) == partnerIds.Count)
            return null;
        return Env.Partners.First(p => p.Id == partnerIds[random.Next(partnerIds.Count)]);
    }

    private decimal GetAmount(Random random)
    {
        return (decimal)random.NextDouble() * 2000 - 1000;
    }

    private decimal GetAmountCurrency(Random random, decimal amount, Core.Currency foreignCurrency)
    {
        return foreignCurrency != null ? (decimal)random.NextDouble() * 9.9m * amount + 0.1m * amount : 0;
    }

    private Core.Currency GetCurrency(Random random, int journalId)
    {
        var journal = Env.Journals.First(j => j.Id == journalId);
        var currency = Env.Currencies.Where(c => c.Active).OrderBy(_ => random.Next()).First();
        return currency.Id != (journal.Currency ?? journal.Company.Currency).Id ? currency : null;
    }

    public void PopulateFactories()
    {
        var random = new Random(Env.GetSeed("account_bank_statement_line+Populate"));

        var companyIds = Env.Companies.Where(c => c.ChartTemplate != null).Select(c => c.Id).ToList();
        var journalIds = Env.Journals.Where(j => companyIds.Contains(j.Company.Id) && (j.Type == "cash" || j.Type == "bank")).Select(j => j.Id).ToList();

        foreach (var journalId in journalIds)
        {
            var newLine = new BankStatementLine
            {
                Journal = Env.Journals.First(j => j.Id == journalId),
                Partner = GetPartner(random, journalId),
                Date = DateTime.Now.AddYears(-4).AddDays(random.Next(1461)),
                PaymentRef = $"transaction_{DateTime.Now:yyyyMMdd}_{Env.BankStatementLines.Count() + 1}",
                Amount = GetAmount(random),
            };
            newLine.ForeignCurrency = GetCurrency(random, journalId);
            newLine.AmountCurrency = GetAmountCurrency(random, newLine.Amount, newLine.ForeignCurrency);

            Env.BankStatementLines.Add(newLine);
        }
    }
}
