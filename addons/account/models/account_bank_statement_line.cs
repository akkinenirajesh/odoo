csharp
public partial class BankStatementLine
{
    public override string ToString()
    {
        return $"{PaymentRef} - {Amount}";
    }

    public void ActionUndoReconciliation()
    {
        // Logic for undoing reconciliation
        // Note: This is a simplified representation. Actual implementation would involve more complex logic
        foreach (var line in LineIds)
        {
            line.RemoveMoveReconcile();
        }
        foreach (var payment in PaymentIds)
        {
            payment.Delete();
        }

        var lineVals = PrepareMoveLinesDefaultVals();
        Write(new Dictionary<string, object>
        {
            { "ToCheck", false },
            { "LineIds", new List<object> { Command.Clear() }.Concat(lineVals.Select(lv => Command.Create(lv))) }
        });
    }

    private BankAccount FindOrCreateBankAccount()
    {
        // Logic for finding or creating a bank account
        var bankAccount = Env.Set<BankAccount>()
            .Search(new List<object>
            {
                new List<object> { "AccNumber", "=", AccountNumber },
                new List<object> { "PartnerId", "=", PartnerId.Id }
            })
            .FirstOrDefault();

        if (bankAccount == null)
        {
            bankAccount = Env.Set<BankAccount>().Create(new Dictionary<string, object>
            {
                { "AccNumber", AccountNumber },
                { "PartnerId", PartnerId.Id },
                { "JournalId", null }
            });
        }

        return bankAccount;
    }

    private List<Dictionary<string, object>> PrepareMoveLinesDefaultVals(int? counterpartAccountId = null)
    {
        // Logic for preparing default values for move lines
        // This is a simplified version. The actual implementation would be more complex
        var result = new List<Dictionary<string, object>>();

        if (!counterpartAccountId.HasValue)
        {
            counterpartAccountId = JournalId.SuspenseAccountId.Id;
        }

        if (!counterpartAccountId.HasValue)
        {
            throw new UserException($"You can't create a new statement line without a suspense account set on the {JournalId.DisplayName} journal.");
        }

        var companyCurrency = JournalId.CompanyId.Currency;
        var journalCurrency = JournalId.CurrencyId ?? companyCurrency;
        var foreignCurrency = ForeignCurrencyId ?? journalCurrency ?? companyCurrency;

        // Add liquidity line values
        result.Add(new Dictionary<string, object>
        {
            { "Name", PaymentRef },
            { "MoveId", MoveId.Id },
            { "PartnerId", PartnerId.Id },
            { "AccountId", JournalId.DefaultAccountId.Id },
            { "CurrencyId", journalCurrency.Id },
            { "AmountCurrency", Amount },
            { "Debit", Amount > 0 ? Amount : 0 },
            { "Credit", Amount < 0 ? -Amount : 0 }
        });

        // Add counterpart line values
        result.Add(new Dictionary<string, object>
        {
            { "Name", PaymentRef },
            { "AccountId", counterpartAccountId.Value },
            { "MoveId", MoveId.Id },
            { "PartnerId", PartnerId.Id },
            { "CurrencyId", foreignCurrency.Id },
            { "AmountCurrency", -AmountCurrency },
            { "Debit", Amount < 0 ? -Amount : 0 },
            { "Credit", Amount > 0 ? Amount : 0 }
        });

        return result;
    }
}
