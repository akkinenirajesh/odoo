csharp
public partial class AccountMove
{
    public void ComputeDebitCount()
    {
        var debitData = Env.AccountMove.ReadGroup(
            new[] { ("DebitOriginId", "in", new[] { this.Id }) },
            new[] { "DebitOriginId" },
            new[] { "__count" }
        );

        var dataMap = debitData.ToDictionary(
            item => item.DebitOriginId.Id,
            item => item.__count
        );

        this.DebitNoteCount = dataMap.GetValueOrDefault(this.Id, 0);
    }

    public ActionResult ActionViewDebitNotes()
    {
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Debit Notes",
            ResModel = "Account.AccountMove",
            ViewMode = "tree,form",
            Domain = new[] { ("DebitOriginId", "=", this.Id) }
        };
    }

    public ActionResult ActionDebitNote()
    {
        var action = Env.Ref("account_debit_note.action_view_account_move_debit").Read()[0];
        return action;
    }
}
