csharp
public partial class AccountSaleClosing
{
    public override string ToString()
    {
        return Name;
    }

    public Dictionary<string, object> ComputeAmounts(ClosingFrequency frequency, Core.Company company)
    {
        var intervalDates = IntervalDates(frequency, company);
        var previousClosing = Env.AccountSaleClosing.Search(new[]
        {
            ("Frequency", "=", frequency),
            ("Company", "=", company.Id)
        }).OrderByDescending(c => c.SequenceNumber).FirstOrDefault();

        var firstOrder = Env.Pos.PosOrder.Empty;
        var dateStart = intervalDates["IntervalFrom"];
        var cumulativeTotal = 0m;

        if (previousClosing != null)
        {
            firstOrder = previousClosing.LastOrder;
            dateStart = previousClosing.CreateDate;
            cumulativeTotal += previousClosing.CumulativeTotal;
        }

        var domain = new List<object[]>
        {
            new object[] { "Company", "=", company.Id },
            new object[] { "State", "in", new[] { "paid", "done", "invoiced" } }
        };

        if (firstOrder.L10nFrSecureSequenceNumber.HasValue)
        {
            domain.Add(new object[] { "L10nFrSecureSequenceNumber", ">", firstOrder.L10nFrSecureSequenceNumber });
        }
        else if (dateStart.HasValue)
        {
            domain.Add(new object[] { "DateOrder", ">=", dateStart });
        }

        var orders = Env.Pos.PosOrder.Search(domain).OrderByDescending(o => o.DateOrder);

        var totalInterval = orders.Sum(o => o.AmountTotal);
        cumulativeTotal += totalInterval;

        var lastOrder = orders.Any() ? orders.First() : firstOrder;

        return new Dictionary<string, object>
        {
            { "TotalInterval", totalInterval },
            { "CumulativeTotal", cumulativeTotal },
            { "LastOrder", lastOrder.Id },
            { "LastOrderHash", lastOrder.L10nFrSecureSequenceNumber },
            { "DateClosingStop", intervalDates["DateStop"] },
            { "DateClosingStart", dateStart },
            { "Name", $"{intervalDates["NameInterval"]} - {((DateTime)intervalDates["DateStop"]).ToString("yyyy-MM-dd")}" }
        };
    }

    private Dictionary<string, object> IntervalDates(ClosingFrequency frequency, Core.Company company)
    {
        var dateStop = DateTime.UtcNow;
        DateTime? intervalFrom = null;
        string nameInterval = "";

        switch (frequency)
        {
            case ClosingFrequency.Daily:
                intervalFrom = dateStop.AddDays(-1);
                nameInterval = "Daily Closing";
                break;
            case ClosingFrequency.Monthly:
                intervalFrom = dateStop.AddMonths(-1);
                nameInterval = "Monthly Closing";
                break;
            case ClosingFrequency.Annually:
                intervalFrom = dateStop.AddYears(-1);
                nameInterval = "Annual Closing";
                break;
        }

        return new Dictionary<string, object>
        {
            { "IntervalFrom", intervalFrom },
            { "DateStop", dateStop },
            { "NameInterval", nameInterval }
        };
    }

    public void Write(Dictionary<string, object> vals)
    {
        throw new UserException("Sale Closings are not meant to be written or deleted under any circumstances.");
    }

    public void Unlink()
    {
        throw new UserException("Sale Closings are not meant to be written or deleted under any circumstances.");
    }

    public static IEnumerable<AccountSaleClosing> AutomatedClosing(ClosingFrequency frequency = ClosingFrequency.Daily)
    {
        var companies = Env.Core.Company.Search(new object[] {});
        var accountClosings = new List<AccountSaleClosing>();

        foreach (var company in companies.Where(c => c.IsAccountingUnalterable()))
        {
            var newSequenceNumber = company.L10nFrClosingSequence.Next();
            var values = ComputeAmounts(frequency, company);
            values["Frequency"] = frequency;
            values["Company"] = company.Id;
            values["SequenceNumber"] = newSequenceNumber;
            accountClosings.Add(Env.Account.AccountSaleClosing.Create(values));
        }

        return accountClosings;
    }
}
