csharp
public partial class SpreadsheetDashboard {
    public object GetReadonlyDashboard() {
        var snapshot = Json.Deserialize<object>(this.SpreadsheetData);
        var userLocale = Env.Get("res.lang")._GetSpreadsheetLocale();
        ((Dictionary<string, object>)snapshot).TryAdd("settings", new Dictionary<string, object>());
        ((Dictionary<string, object>)snapshot["settings"])["locale"] = userLocale;
        var defaultCurrency = Env.Get("res.currency").GetCompanyCurrencyForSpreadsheet();
        return new {
            snapshot = snapshot,
            revisions = new List<object>(),
            defaultCurrency = defaultCurrency,
        };
    }

    public List<object> CopyData(object defaultValues = null) {
        var valsList = base.CopyData(defaultValues);
        if (defaultValues == null || !((Dictionary<string, object>)defaultValues).ContainsKey("Name")) {
            for (var i = 0; i < this.Count; i++) {
                ((Dictionary<string, object>)valsList[i])["Name"] = $"%s (copy)";
            }
        }
        return valsList;
    }
}
