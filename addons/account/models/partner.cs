csharp
public partial class AccountFiscalPosition
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeStatesCount()
    {
        StatesCount = Country?.States?.Count() ?? 0;
    }

    public void ComputeForeignVatHeaderMode()
    {
        if (string.IsNullOrEmpty(ForeignVat) || Country == null || 
            Env.AccountTax.Search(new[] { ("Country", "=", Country) }, limit: 1).Any())
        {
            ForeignVatHeaderMode = null;
        }
        else
        {
            var templateCode = Env.AccountChartTemplate.GuessChartTemplate(Country);
            var template = Env.AccountChartTemplate.GetChartTemplateMapping()[templateCode];
            ForeignVatHeaderMode = template.Installed ? 
                ForeignVatHeaderMode.TemplatesFound : ForeignVatHeaderMode.NoTemplate;
        }
    }

    public void ComputeTaxMap()
    {
        var taxMap = new Dictionary<int, List<int>>();
        foreach (var taxMapping in TaxMappings)
        {
            if (taxMapping.TaxDest != null)
            {
                if (!taxMap.ContainsKey(taxMapping.TaxSrc.Id))
                {
                    taxMap[taxMapping.TaxSrc.Id] = new List<int>();
                }
                taxMap[taxMapping.TaxSrc.Id].Add(taxMapping.TaxDest.Id);
            }
            else
            {
                taxMap[taxMapping.TaxSrc.Id] = new List<int>();
            }
        }
        TaxMap = SerializeDictionary(taxMap);
    }

    public void ComputeAccountMap()
    {
        var accountMap = AccountMappings.ToDictionary(
            al => al.AccountSrc.Id,
            al => al.AccountDest.Id
        );
        AccountMap = SerializeDictionary(accountMap);
    }

    private byte[] SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        // Implement serialization logic here
        // This is a placeholder and should be replaced with actual implementation
        return new byte[0];
    }
}
