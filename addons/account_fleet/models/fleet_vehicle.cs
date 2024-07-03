csharp
public partial class FleetVehicle
{
    public int ComputeMoveIds()
    {
        if (!Env.User.HasGroup("account.group_account_readonly"))
        {
            AccountMoveIds = new Account.AccountMove[0];
            BillCount = 0;
            return 0;
        }

        var moves = Env.AccountMoveLine.ReadGroup(
            domain: new[]
            {
                ("VehicleId", "in", new[] { Id }),
                ("ParentState", "!=", "cancel"),
                ("MoveId.MoveType", "in", Env.AccountMove.GetPurchaseTypes())
            },
            groupBy: new[] { "VehicleId" },
            aggregates: new[] { "MoveId:array_agg" }
        );

        var vehicleMoveMapping = moves.ToDictionary(
            m => m.VehicleId,
            m => new HashSet<int>(m.MoveIds)
        );

        if (vehicleMoveMapping.TryGetValue(Id, out var moveIds))
        {
            AccountMoveIds = Env.AccountMove.Browse(moveIds.ToArray());
            BillCount = AccountMoveIds.Length;
        }
        else
        {
            AccountMoveIds = new Account.AccountMove[0];
            BillCount = 0;
        }

        return BillCount;
    }

    public Dictionary<string, object> ActionViewBills()
    {
        var formViewRef = Env.Ref("account.view_move_form");
        var treeViewRef = Env.Ref("account_fleet.account_move_view_tree");

        var result = Env.IrActionsActWindow.ForXmlId("account.action_move_in_invoice_type");
        result["domain"] = new[] { ("Id", "in", AccountMoveIds.Select(m => m.Id).ToArray()) };
        result["views"] = new[]
        {
            new object[] { treeViewRef.Id, "tree" },
            new object[] { formViewRef.Id, "form" }
        };

        return result;
    }
}
