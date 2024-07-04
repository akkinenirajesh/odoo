csharp
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Contract
{
    public void CheckContracts()
    {
        GetLeaves().CheckContracts();
    }

    public List<Hr.Leave> GetLeaves()
    {
        return Env.Set<Hr.Leave>().Search(new List<object[]>
        {
            new object[] { "State", "!=", "Refuse" },
            new object[] { "Employee", "=", this.Employee.Id },
            new object[] { "DateFrom", "<=", this.DateEnd ?? DateTime.MaxValue },
            new object[] { "DateTo", ">=", this.DateStart }
        });
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        if (!(vals.ContainsKey("State") && (string)vals["State"] == "Open") &&
            !(vals.ContainsKey("KanbanState") && (Hr.KanbanState)vals["KanbanState"] == Hr.KanbanState.Done))
        {
            return base.Write(vals);
        }

        var leaves = GetLeaves();
        var leavesState = new Dictionary<int, string>();

        try
        {
            foreach (var leave in leaves)
            {
                var overlappingContracts = leave.GetOverlappingContracts(new List<object[]>
                {
                    new object[] { "State", "!=", "Cancel" },
                    new object[] { "ResourceCalendar", "!=", null },
                    new object[] { "Id", "=", this.Id },
                    new object[] { "State", "!=", "Draft" },
                    new object[] { "KanbanState", "=", "Done" }
                }).OrderBy(c => new Dictionary<string, int> {
                    {"Open", 1}, {"Close", 2}, {"Draft", 3}, {"Cancel", 4}
                }[c.State]);

                if (overlappingContracts.Select(c => c.ResourceCalendar).Distinct().Count() <= 1)
                {
                    if (overlappingContracts.Any() && leave.ResourceCalendar != overlappingContracts.First().ResourceCalendar)
                    {
                        leave.ResourceCalendar = overlappingContracts.First().ResourceCalendar;
                    }
                    continue;
                }

                if (!leavesState.ContainsKey(leave.Id))
                {
                    leavesState[leave.Id] = leave.State;
                }

                if (leave.State != "Refuse")
                {
                    leave.ActionRefuse();
                }

                base.Write(vals);

                foreach (var overlappingContract in overlappingContracts)
                {
                    var newRequestDateFrom = Max(leave.RequestDateFrom, overlappingContract.DateStart);
                    var newRequestDateTo = Min(leave.RequestDateTo, overlappingContract.DateEnd ?? DateTime.MaxValue);

                    var newLeaveVals = new Dictionary<string, object>
                    {
                        { "RequestDateFrom", newRequestDateFrom },
                        { "RequestDateTo", newRequestDateTo },
                        { "State", leavesState[leave.Id] }
                    };

                    var newLeave = Env.New<Hr.Leave>(newLeaveVals);
                    newLeave.ComputeDateFromTo();
                    newLeave.ComputeDuration();

                    if (newLeave.DateFrom < newLeave.DateTo)
                    {
                        // Create new leave logic here
                    }
                }
            }
        }
        catch (ValidationException)
        {
            throw new ValidationException("Changing the contract on this employee changes their working schedule in a period " +
                "they already took leaves. Changing this working schedule changes the duration of " +
                "these leaves in such a way the employee no longer has the required allocation for " +
                "them. Please review these leaves and/or allocations before changing the contract.");
        }

        return base.Write(vals);
    }

    private DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    private DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
