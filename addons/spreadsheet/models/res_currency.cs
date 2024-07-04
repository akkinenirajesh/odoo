csharp
public partial class Spreadsheet.ResCurrency {

    public Dictionary<string, object> GetCompanyCurrencyForSpreadsheet(int companyId) {
        var company = Env.GetModel("Res.Company").Get(companyId);
        if (company == null) {
            return null;
        }
        var currency = Env.GetModel("Res.Currency").Get(company.CurrencyId);
        return new Dictionary<string, object> {
            {"Code", currency.Name},
            {"Symbol", currency.Symbol},
            {"DecimalPlaces", currency.DecimalPlaces},
            {"Position", currency.Position}
        };
    }
}
