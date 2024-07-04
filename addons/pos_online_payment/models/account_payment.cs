csharp
public partial class AccountPayment {
    public Action ActionViewPosOrder() {
        var action = new Dictionary<string, object> {
            { "Name", "POS Order" },
            { "Type", "ir.actions.act_window" },
            { "ResModel", "Pos.PosOrder" },
            { "Target", "current" },
            { "ResId", this.PosOrderId.Id },
            { "ViewMode", "form" },
        };
        return action;
    }
}
