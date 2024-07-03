csharp
public partial class AccountFullReconcile
{
    public bool Unlink()
    {
        // Avoid cyclic unlink calls when removing partials.
        if (this == null)
        {
            return true;
        }

        var movesToReverse = this.ExchangeMoveId;

        bool res = base.Unlink();

        // Reverse all exchange moves at once.
        if (movesToReverse != null)
        {
            var defaultValuesList = new List<Dictionary<string, object>>();
            foreach (var move in movesToReverse)
            {
                defaultValuesList.Add(new Dictionary<string, object>
                {
                    ["Date"] = move.GetAccountingDate(move.Date, move.AffectTaxReport()),
                    ["Ref"] = $"Reversal of: {move.Name}"
                });
            }
            movesToReverse.ReverseMoves(defaultValuesList, cancel: true);
        }

        return res;
    }

    public static List<AccountFullReconcile> Create(List<Dictionary<string, object>> valsList)
    {
        List<List<long>> moveLineIds = new List<List<long>>();
        List<List<long>> partialIds = new List<List<long>>();

        foreach (var vals in valsList)
        {
            moveLineIds.Add(GetIds((List<object>)vals["ReconciledLineIds"]));
            partialIds.Add(GetIds((List<object>)vals["PartialReconcileIds"]));
            vals.Remove("ReconciledLineIds");
            vals.Remove("PartialReconcileIds");
        }

        var fulls = base.Create(valsList);

        // Update account_move_line
        Env.Cr.ExecuteValues(@"
            UPDATE account_move_line line
               SET full_reconcile_id = source.full_id
              FROM (VALUES %s) AS source(full_id, line_ids)
             WHERE line.id = ANY(source.line_ids)
        ", fulls.Zip(moveLineIds, (full, lineIds) => new object[] { full.Id, lineIds }).ToList());

        fulls.ForEach(f => f.ReconciledLineIds.InvalidateRecordset(new[] { "FullReconcileId" }, flush: false));
        fulls.ForEach(f => f.InvalidateRecordset(new[] { "ReconciledLineIds" }, flush: false));

        // Update account_partial_reconcile
        Env.Cr.ExecuteValues(@"
            UPDATE account_partial_reconcile partial
               SET full_reconcile_id = source.full_id
              FROM (VALUES %s) AS source(full_id, partial_ids)
             WHERE partial.id = ANY(source.partial_ids)
        ", fulls.Zip(partialIds, (full, pIds) => new object[] { full.Id, pIds }).ToList());

        fulls.ForEach(f => f.PartialReconcileIds.InvalidateRecordset(new[] { "FullReconcileId" }, flush: false));
        fulls.ForEach(f => f.InvalidateRecordset(new[] { "PartialReconcileIds" }, flush: false));

        Env.Get<AccountPartialReconcile>().UpdateMatchingNumber(fulls.SelectMany(f => f.ReconciledLineIds).ToList());

        return fulls;
    }

    private static List<long> GetIds(List<object> commands)
    {
        var result = new List<long>();
        foreach (var command in commands)
        {
            var cmd = (List<object>)command;
            int cmdType = (int)cmd[0];
            if (cmdType == 4) // LINK
            {
                result.Add((long)cmd[1]);
            }
            else if (cmdType == 6) // SET
            {
                result.AddRange((List<long>)cmd[2]);
            }
            else
            {
                throw new ArgumentException($"Unexpected command: {cmdType}");
            }
        }
        return result;
    }
}
