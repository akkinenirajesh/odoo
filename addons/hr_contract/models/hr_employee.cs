csharp
public partial class Employee
{
    public DateTime? GetFirstContractDate(bool noGap = true)
    {
        var contracts = GetFirstContracts().OrderByDescending(c => c.DateStart);
        if (noGap)
        {
            contracts = RemoveGap(contracts);
        }
        return contracts.Any() ? contracts.Min(c => c.DateStart) : (DateTime?)null;
    }

    private IEnumerable<Hr.Contract> RemoveGap(IEnumerable<Hr.Contract> contracts)
    {
        // Implementation of remove_gap logic
        // This is a simplified version and may need adjustment
        if (!contracts.Any()) return Enumerable.Empty<Hr.Contract>();
        if (contracts.Count() == 1) return contracts;

        var result = new List<Hr.Contract>();
        var currentContract = contracts.First();
        var olderContracts = contracts.Skip(1);
        var currentDate = currentContract.DateStart;

        foreach (var otherContract in olderContracts)
        {
            var gap = (currentDate - (otherContract.DateEnd ?? new DateTime(2100, 1, 1))).Days;
            currentDate = otherContract.DateStart;
            if (gap >= 4)
            {
                result.AddRange(olderContracts.Take(result.Count));
                result.Add(currentContract);
                return result;
            }
            result.Add(otherContract);
        }

        result.Add(currentContract);
        return result;
    }

    public IEnumerable<Hr.Contract> GetContracts(DateTime dateFrom, DateTime dateTo, IEnumerable<string> states = null, IEnumerable<string> kanbanState = null)
    {
        states = states ?? new[] { "open" };
        var contractDomain = new List<object>
        {
            new List<object> { "EmployeeId", "=", Id },
            new List<object> { "State", "in", states },
            new List<object> { "DateStart", "<=", dateTo },
            new List<object>
            {
                "|",
                new List<object> { "DateEnd", "=", false },
                new List<object> { "DateEnd", ">=", dateFrom }
            }
        };

        if (kanbanState != null)
        {
            contractDomain.Add(new List<object> { "KanbanState", "in", kanbanState });
        }

        return Env.Pool.Get<Hr.Contract>().Search(contractDomain);
    }

    public override string ToString()
    {
        // Implement logic to return a string representation of the employee
        return Name;
    }
}
