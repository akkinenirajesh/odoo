csharp
public partial class ResCompany
{
    public override string ToString()
    {
        return Name;
    }

    public void ReflectCodePrefixChange(string oldCode, string newCode)
    {
        if (string.IsNullOrEmpty(oldCode) || newCode == oldCode)
            return;

        var accounts = Env.Query<Account.Account>()
            .Where(a => a.Company == this && a.Code.StartsWith(oldCode) && 
                   (a.AccountType == "AssetCash" || a.AccountType == "LiabilityCreditCard"))
            .OrderBy(a => a.Code)
            .ToList();

        foreach (var account in accounts)
        {
            account.Code = GetNewAccountCode(account.Code, oldCode, newCode);
        }
    }

    public string GetNewAccountCode(string currentCode, string oldPrefix, string newPrefix)
    {
        int digits = currentCode.Length;
        return newPrefix + currentCode.Replace(oldPrefix, "").TrimStart('0').PadLeft(digits - newPrefix.Length, '0');
    }

    public Dictionary<string, DateTime> ComputeFiscalyearDates(DateTime currentDate)
    {
        return new Dictionary<string, DateTime>
        {
            { "DateFrom", new DateTime(currentDate.Year, 1, 1) },
            { "DateTo", new DateTime(currentDate.Year, 12, 31) }
        };
    }

    public bool ExistingAccounting()
    {
        return Env.Query<Account.MoveLine>().Where(ml => ml.Company == this).Any();
    }

    public List<object> ChartTemplateSelection()
    {
        return Env.Ref<Account.ChartTemplate>().SelectChartTemplate(Country);
    }

    public object ActionCheckHashIntegrity()
    {
        return Env.Ref<Core.Action>("Account.ActionReportAccountHashIntegrity").ReportAction(Id);
    }
}
