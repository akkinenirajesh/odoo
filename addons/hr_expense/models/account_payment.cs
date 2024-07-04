csharp
public partial class AccountPayment
{
    public Dictionary<string, object> ActionOpenExpenseReport()
    {
        return new Dictionary<string, object>
        {
            ["name"] = this.ExpenseSheetId.Name,
            ["type"] = "ir.actions.act_window",
            ["view_type"] = "form",
            ["view_mode"] = "form",
            ["views"] = new List<object> { new List<object> { false, "form" } },
            ["res_model"] = "Hr.ExpenseSheet",
            ["res_id"] = this.ExpenseSheetId.Id
        };
    }

    public void SynchronizeFromMoves(HashSet<string> changedFields)
    {
        if (this.ExpenseSheetId != null)
        {
            // Constraints bypass when entry is linked to an expense.
            // Context is not enough, as we want to be able to delete
            // and update those entries later on.
            return;
        }
        // Call base implementation
        base.SynchronizeFromMoves(changedFields);
    }

    public void SynchronizeToMoves(HashSet<string> changedFields)
    {
        var triggerFields = new HashSet<string>(GetTriggerFieldsToSynchronize()) { "Ref", "ExpenseSheetId", "PaymentMethodLineId" };
        if (this.ExpenseSheetId != null && changedFields.Intersect(triggerFields).Any())
        {
            throw new UserException("You cannot do this modification since the payment is linked to an expense report.");
        }
        // Call base implementation
        base.SynchronizeToMoves(changedFields);
    }

    public string CreationMessage()
    {
        if (this.MoveId.ExpenseSheetId != null)
        {
            return $"Payment created for: {this.MoveId.ExpenseSheetId.GetHtmlLink()}";
        }
        // Call base implementation
        return base.CreationMessage();
    }

    public void MustDeleteAllExpensePayments()
    {
        if (this.ExpenseSheetId != null && this.ExpenseSheetId.AccountMoveIds.PaymentIds.Except(new[] { this }).Any())
        {
            throw new UserException("You cannot delete only some payments linked to an expense report. All payments must be deleted at the same time.");
        }
    }

    private HashSet<string> GetTriggerFieldsToSynchronize()
    {
        // Implementation details would depend on the base class and overall architecture
        throw new NotImplementedException();
    }
}
