csharp
public partial class Account
{
    public virtual bool IncludeInitialBalance { get; set; }
}

public partial class Account
{
    public virtual IEnumerable<Account> Search(string domain = null, int limit = 0, string order = null, int offset = 0)
    {
        return Env.Model<Account>().Search(domain, limit, order, offset);
    }

    public virtual IEnumerable<Account> ReadGroup(string domain = null, List<string> fields = null, string groupBy = null)
    {
        return Env.Model<Account>().ReadGroup(domain, fields, groupBy);
    }

    public virtual Account Browse(int id)
    {
        return Env.Model<Account>().Browse(id);
    }

    public virtual Account Browse(string domain = null)
    {
        return Env.Model<Account>().Browse(domain);
    }

    public virtual IEnumerable<Account> Search(Dictionary<string, object> criteria)
    {
        return Env.Model<Account>().Search(criteria);
    }

    public virtual List<Account> GetAccountGroup(List<string> accountTypes)
    {
        var data = ReadGroup(new List<string>()
        {
            $"{this.GetTableName()}.Company = {Env.Company.Id}",
            $"AccountType in ({string.Join(",", accountTypes)})"
        }, new List<string>() { "AccountType" }, "Code:array_agg");

        var mapped = data.ToDictionary(item => item["AccountType"], item => item["Code"]);
        return accountTypes.Select(accountType => mapped.ContainsKey(accountType) ? mapped[accountType] : new List<string>()).ToList();
    }

    private string GetTableName()
    {
        return this.GetType().Name.Split('.')[1];
    }
}

public partial class Move
{
    public virtual string State { get; set; }
    public virtual Core.Company Company { get; set; }
}

public partial class MoveLine
{
    public virtual DateTime Date { get; set; }
    public virtual Account Account { get; set; }
    public virtual Move Move { get; set; }
    public virtual decimal Debit { get; set; }
    public virtual decimal Credit { get; set; }
    public virtual Core.Company Company { get; set; }
}

public partial class Account
{
    public virtual (DateTime, DateTime) GetDatePeriodBoundaries(Dictionary<string, object> datePeriod, Core.Company company)
    {
        string periodType = datePeriod["range_type"].ToString();
        int year = int.Parse(datePeriod["year"].ToString());
        int month = int.Parse(datePeriod["month"].ToString());
        int quarter = int.Parse(datePeriod["quarter"].ToString());
        int day = int.Parse(datePeriod["day"].ToString());

        if (periodType == "year")
        {
            int fiscalDay = company.FiscalYearLastDay;
            int fiscalMonth = int.Parse(company.FiscalYearLastMonth);
            if (fiscalDay != 31 || fiscalMonth != 12)
            {
                year += 1;
            }

            int maxDay = DateTime.DaysInMonth(year, fiscalMonth);
            DateTime current = new DateTime(year, fiscalMonth, Math.Min(fiscalDay, maxDay));
            return DateUtils.GetFiscalYear(current, fiscalDay, fiscalMonth);
        }
        else if (periodType == "month")
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }
        else if (periodType == "quarter")
        {
            int firstMonth = quarter * 3 - 2;
            DateTime start = new DateTime(year, firstMonth, 1);
            DateTime end = start.AddMonths(3).AddDays(-1);
            return (start, end);
        }
        else if (periodType == "day")
        {
            int fiscalDay = company.FiscalYearLastDay;
            int fiscalMonth = int.Parse(company.FiscalYearLastMonth);
            DateTime end = new DateTime(year, month, day);
            (DateTime start, _) = DateUtils.GetFiscalYear(end, fiscalDay, fiscalMonth);
            return (start, end);
        }
        return (DateTime.MinValue, DateTime.MinValue);
    }

    public virtual Dictionary<string, object> BuildSpreadsheetFormulaDomain(Dictionary<string, object> formulaParams)
    {
        var codes = formulaParams["codes"] as List<string>;
        if (codes == null || codes.Count == 0)
        {
            return new Dictionary<string, object>() { { "domain", new List<string>() { "False" } } };
        }

        int companyID = (int)formulaParams["company_id"] ?? Env.Company.Id;
        Core.Company company = Env.Model<Core.Company>().Browse(companyID);

        (DateTime start, DateTime end) = GetDatePeriodBoundaries((Dictionary<string, object>)formulaParams["date_range"], company);

        var balanceDomain = new List<string>()
        {
            $"{this.GetTableName()}.IncludeInitialBalance = True",
            $"Date <= {end:yyyy-MM-dd}"
        };

        var pnlDomain = new List<string>()
        {
            $"{this.GetTableName()}.IncludeInitialBalance = False",
            $"Date >= {start:yyyy-MM-dd}",
            $"Date <= {end:yyyy-MM-dd}"
        };

        var codeDomain = codes.Select(code => $"Code like '{code}%'").ToList();
        var accountIds = Env.Model<Account>().Search(string.Join(" OR ", codeDomain)).Select(a => a.Id).ToList();
        var domain = new List<string>()
        {
            $"Account in ({string.Join(",", accountIds)})",
            $"Company = {companyID}"
        };

        domain.AddRange(new List<string>() { $"{string.Join(" OR ", balanceDomain)}", $"{string.Join(" OR ", pnlDomain)}" });

        if ((bool)formulaParams["include_unposted"])
        {
            domain.Add($"Move.State != 'cancel'");
        }
        else
        {
            domain.Add($"Move.State = 'posted'");
        }

        return new Dictionary<string, object>() { { "domain", domain } };
    }

    public virtual Dictionary<string, object> SpreadsheetMoveLineAction(Dictionary<string, object> args)
    {
        var domain = BuildSpreadsheetFormulaDomain(args)["domain"] as List<string>;
        return new Dictionary<string, object>()
        {
            { "type", "ir.actions.act_window" },
            { "res_model", "account.move.line" },
            { "view_mode", "list" },
            { "views", new List<List<object>>() { new List<object>() { false, "list" } } },
            { "target", "current" },
            { "domain", domain },
            { "name", $"Journal items for account prefix {string.Join(", ", args["codes"] as List<string>)}" }
        };
    }

    public virtual List<Dictionary<string, object>> SpreadsheetFetchDebitCredit(List<Dictionary<string, object>> argsList)
    {
        var results = new List<Dictionary<string, object>>();
        foreach (var args in argsList)
        {
            int companyID = (int)args["company_id"] ?? Env.Company.Id;
            var domain = BuildSpreadsheetFormulaDomain(args)["domain"] as List<string>;
            var MoveLines = Env.Model<MoveLine>().WithCompany(companyID);
            var debitCredit = MoveLines.ReadGroup(domain, new List<string>() { "Debit:sum", "Credit:sum" }, null)[0];
            results.Add(new Dictionary<string, object>()
            {
                { "debit", (decimal)debitCredit["Debit"] ?? 0 },
                { "credit", (decimal)debitCredit["Credit"] ?? 0 }
            });
        }
        return results;
    }

    private string GetTableName()
    {
        return this.GetType().Name.Split('.')[1];
    }
}
