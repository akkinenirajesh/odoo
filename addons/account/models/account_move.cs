csharp
public partial class AccountMove
{
    public override string ToString()
    {
        return Name;
    }

    public AccountMove Copy(Dictionary<string, object> default_values = null)
    {
        // Implementation for copying the move
        // This would create a new AccountMove object with copied values
        // and handle special cases like resetting certain fields
        throw new NotImplementedException();
    }

    public void Post()
    {
        // Implementation for posting the move
        if (State != AccountMoveState.Draft)
        {
            throw new UserError("You can only post draft moves.");
        }
        // Additional posting logic
        State = AccountMoveState.Posted;
    }

    public void Button_Draft()
    {
        // Implementation for resetting to draft
        if (State != AccountMoveState.Posted && State != AccountMoveState.Cancel)
        {
            throw new UserError("Only posted or cancelled entries can be reset to draft.");
        }
        // Additional logic for resetting to draft
        State = AccountMoveState.Draft;
    }

    public void Button_Cancel()
    {
        // Implementation for cancelling the move
        if (State != AccountMoveState.Draft)
        {
            throw new UserError("Only draft moves can be cancelled.");
        }
        State = AccountMoveState.Cancel;
    }

    public bool IsInvoice(bool include_receipts = false)
    {
        return IsSaleDocument(include_receipts) || IsPurchaseDocument(include_receipts);
    }

    public bool IsSaleDocument(bool include_receipts = false)
    {
        var sale_types = new[] { AccountMoveType.OutInvoice, AccountMoveType.OutRefund };
        if (include_receipts)
        {
            sale_types = sale_types.Append(AccountMoveType.OutReceipt).ToArray();
        }
        return sale_types.Contains(MoveType);
    }

    public bool IsPurchaseDocument(bool include_receipts = false)
    {
        var purchase_types = new[] { AccountMoveType.InInvoice, AccountMoveType.InRefund };
        if (include_receipts)
        {
            purchase_types = purchase_types.Append(AccountMoveType.InReceipt).ToArray();
        }
        return purchase_types.Contains(MoveType);
    }

    public decimal GetBalanceMultiplicator()
    {
        return IsInbound() ? -1 : 1;
    }

    public bool IsInbound(bool include_receipts = true)
    {
        return MoveType == AccountMoveType.OutInvoice
            || MoveType == AccountMoveType.InRefund
            || (include_receipts && MoveType == AccountMoveType.OutReceipt);
    }

    public bool IsOutbound(bool include_receipts = true)
    {
        return MoveType == AccountMoveType.InInvoice
            || MoveType == AccountMoveType.OutRefund
            || (include_receipts && MoveType == AccountMoveType.InReceipt);
    }

    // Additional methods would be implemented here
}
