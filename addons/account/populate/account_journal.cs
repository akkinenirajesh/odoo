csharp
public partial class AccountJournal
{
    public override string ToString()
    {
        return $"{Name} ({Code})";
    }

    public void Populate(PopulateSize size)
    {
        var populateSizes = new Dictionary<PopulateSize, int>
        {
            { PopulateSize.Small, 10 },
            { PopulateSize.Medium, 30 },
            { PopulateSize.Large, 100 }
        };

        int count = populateSizes[size];

        var companies = Env.Set<Core.Company>().Where(c => c.ChartTemplate != null).ToList();
        if (companies.Count == 0)
        {
            return;
        }

        var currencies = Env.Set<Core.Currency>().Where(c => c.Active).ToList();
        currencies.Add(null); // Adding null as an option for currency

        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            var company = companies[random.Next(companies.Count)];
            var type = (JournalType)random.Next(Enum.GetValues(typeof(JournalType)).Length);
            var currency = currencies[random.Next(currencies.Count)];

            Env.Set<Account.AccountJournal>().Create(new AccountJournal
            {
                CompanyId = company,
                Type = type,
                CurrencyId = currency,
                Name = $"Journal {type} {i + 1}",
                Code = $"{type.ToString().Substring(0, 2)}{i + 1:D2}"
            });
        }
    }
}

public enum PopulateSize
{
    Small,
    Medium,
    Large
}
