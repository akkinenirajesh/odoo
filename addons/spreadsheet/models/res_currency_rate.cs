csharp
public partial class Spreadsheet.ResCurrencyRate
{
    public virtual decimal GetRateForSpreadsheet(string currencyFromCode, string currencyToCode, DateTime? date)
    {
        if (string.IsNullOrEmpty(currencyFromCode) || string.IsNullOrEmpty(currencyToCode))
        {
            return 0;
        }
        var currencyFrom = Env.Search<Core.Currency>(c => c.Name == currencyFromCode);
        var currencyTo = Env.Search<Core.Currency>(c => c.Name == currencyToCode);

        if (currencyFrom == null || currencyTo == null)
        {
            return 0;
        }

        var company = Env.Company;
        date = date ?? DateTime.Now;

        return currencyFrom.GetConversionRate(currencyTo, company, date);
    }

    public virtual List<Dictionary<string, object>> GetRatesForSpreadsheet(List<Dictionary<string, object>> requests)
    {
        var result = new List<Dictionary<string, object>>();
        foreach (var request in requests)
        {
            var record = request.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            record["Rate"] = GetRateForSpreadsheet((string)request["From"], (string)request["To"], request.ContainsKey("Date") ? (DateTime?)request["Date"] : null);
            result.Add(record);
        }
        return result;
    }
}
