csharp
public partial class AccountPaymentMethod
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<AccountPaymentMethod> Create(IEnumerable<AccountPaymentMethod> valsList)
    {
        var paymentMethods = base.Create(valsList);
        var methodsInfo = GetPaymentMethodInformation();

        foreach (var method in paymentMethods)
        {
            var information = methodsInfo.TryGetValue(method.Code, out var info) ? info : new Dictionary<string, object>();

            if (information.TryGetValue("mode", out var mode) && (string)mode == "multi")
            {
                var methodDomain = GetPaymentMethodDomain(method.Code);

                var journals = Env.Set<AccountJournal>().Search(methodDomain);

                Env.Set<AccountPaymentMethodLine>().Create(journals.Select(journal => new AccountPaymentMethodLine
                {
                    Name = method.Name,
                    PaymentMethod = method,
                    Journal = journal
                }));
            }
        }

        return paymentMethods;
    }

    public IEnumerable<object> GetPaymentMethodDomain(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return new List<object>();
        }

        var information = GetPaymentMethodInformation().TryGetValue(code, out var info) ? info : new Dictionary<string, object>();

        var currencyIds = information.TryGetValue("currency_ids", out var currencies) ? (List<int>)currencies : null;
        var countryId = information.TryGetValue("country_id", out var country) ? (int)country : 0;
        var defaultDomain = new List<object> { new List<object> { "Type", "in", new List<string> { "bank", "cash" } } };
        var domains = new List<List<object>> { information.TryGetValue("domain", out var domain) ? (List<object>)domain : defaultDomain };

        if (currencyIds != null)
        {
            domains.Add(new List<object>
            {
                "|",
                new List<object> { "Currency", "=", false },
                new List<object> { "Company.Currency", "in", currencyIds },
                new List<object> { "Currency", "in", currencyIds }
            });
        }

        if (countryId != 0)
        {
            domains.Add(new List<object> { new List<object> { "Company.AccountFiscalCountry", "=", countryId } });
        }

        return domains;
    }

    public Dictionary<string, Dictionary<string, object>> GetPaymentMethodInformation()
    {
        return new Dictionary<string, Dictionary<string, object>>
        {
            {
                "manual", new Dictionary<string, object>
                {
                    { "mode", "multi" },
                    { "domain", new List<object> { new List<object> { "Type", "in", new List<string> { "bank", "cash" } } } }
                }
            }
        };
    }

    public List<string> GetSddPaymentMethodCode()
    {
        return new List<string>();
    }
}
