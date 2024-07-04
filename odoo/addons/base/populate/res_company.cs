csharp
public partial class Company
{
    public override string ToString()
    {
        return Name;
    }

    public void Populate(PopulateSize size)
    {
        // Activate currency to avoid fail iterator
        var usd = Env.Ref("base.USD");
        var eur = Env.Ref("base.EUR");
        usd.Active = true;
        eur.Active = true;

        var lastId = Env.Search<Base.Company>(order: "Id desc", limit: 1).FirstOrDefault()?.Id ?? 0;

        var factories = new List<Func<object>>
        {
            () => new { Name = $"company_{++lastId}" },
            () => new { Sequence = Random.Shared.Next(0, 101) },
            () => new { CompanyRegistry = Random.Shared.Next(2) == 0 ? null : $"company_registry_{lastId}" },
            () => new { PrimaryColor = new[] { null, "", "#ff7755" }[Random.Shared.Next(3)] },
            () => new { SecondaryColor = new[] { null, "", "#ffff55" }[Random.Shared.Next(3)] },
            () => new { Currency = Env.Search<Base.Currency>(c => c.Active).RandomElement() }
        };

        var records = PopulateHelper.Generate(size, factories);

        foreach (var record in records)
        {
            record.Name = $"company_{record.Id}_{record.Currency.Name}";
        }

        var adminUser = Env.Ref("base.user_admin");
        adminUser.Companies = adminUser.Companies.Concat(records).ToList();
    }
}
