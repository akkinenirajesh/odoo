csharp
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AccountMove
{
    public void Populate(int size)
    {
        var random = new Random();
        var env = Env;

        var companies = env.Companies.Where(c => c.ChartTemplate != null).ToList();
        var currencies = env.Currencies.Where(c => c.Active).ToList();

        if (companies.Count == 0)
        {
            return;
        }

        for (int i = 0; i < size; i++)
        {
            var values = new Dictionary<string, object>
            {
                ["MoveType"] = GetRandomMoveType(random),
                ["Company"] = companies[random.Next(companies.Count)],
                ["Currency"] = currencies[random.Next(currencies.Count)],
                ["Date"] = GetRandomDate(random),
            };

            values["Journal"] = GetJournal(random, values);
            values["InvoiceDate"] = GetInvoiceDate(values);
            values["Partner"] = GetPartner(random, values);
            values["Lines"] = GetLines(random, values);

            var move = env.AccountMoves.Create(values);

            if (move.Date < DateTime.Today)
            {
                move.ActionPost();
            }
        }
    }

    private Account.MoveType GetRandomMoveType(Random random)
    {
        var moveTypes = Enum.GetValues(typeof(Account.MoveType)).Cast<Account.MoveType>().ToList();
        var weights = new[] { 0.2, 0.3, 0.3, 0.07, 0.07, 0.03, 0.03 };

        double totalWeight = weights.Sum();
        double randomValue = random.NextDouble() * totalWeight;

        for (int i = 0; i < weights.Length; i++)
        {
            if (randomValue < weights[i])
            {
                return moveTypes[i];
            }
            randomValue -= weights[i];
        }

        return moveTypes.Last();
    }

    private DateTime GetRandomDate(Random random)
    {
        var start = DateTime.Today.AddYears(-4);
        var range = (DateTime.Today.AddYears(1) - start).Days;
        return start.AddDays(random.Next(range));
    }

    private Account.Journal GetJournal(Random random, Dictionary<string, object> values)
    {
        // Implementation details omitted for brevity
        // This method should return a random journal based on the company and move type
        throw new NotImplementedException();
    }

    private DateTime? GetInvoiceDate(Dictionary<string, object> values)
    {
        var moveType = (Account.MoveType)values["MoveType"];
        if (IsInvoiceType(moveType))
        {
            return (DateTime)values["Date"];
        }
        return null;
    }

    private Core.Partner GetPartner(Random random, Dictionary<string, object> values)
    {
        // Implementation details omitted for brevity
        // This method should return a random partner based on the company and move type
        throw new NotImplementedException();
    }

    private List<Account.AccountMoveLine> GetLines(Random random, Dictionary<string, object> values)
    {
        // Implementation details omitted for brevity
        // This method should return a list of AccountMoveLine objects
        throw new NotImplementedException();
    }

    private bool IsInvoiceType(Account.MoveType moveType)
    {
        return moveType != Account.MoveType.Entry;
    }

    public void ActionPost()
    {
        // Implementation of posting logic
        throw new NotImplementedException();
    }
}
