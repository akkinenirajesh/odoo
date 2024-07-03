csharp
public partial class AccountPartialReconcile
{
    public override string ToString()
    {
        return $"Partial Reconcile: {DebitMoveId} - {CreditMoveId}";
    }

    public void Unlink()
    {
        // Implementation of unlink method
        // Note: This is a simplified version. You'll need to adapt the logic to C#
        var fullToUnlink = this.FullReconcileId;
        var allReconciled = this.DebitMoveId.Concat(this.CreditMoveId);

        // Retrieve the CABA entries to reverse
        var movesToReverse = Env.GetModel<Account.AccountMove>().Search(new[] { ("TaxCashBasisRecId", "in", new[] { this.Id }) });
        movesToReverse = movesToReverse.Concat(this.ExchangeMoveId);

        // Actual unlink operation would go here

        fullToUnlink.Unlink();

        if (movesToReverse.Any())
        {
            foreach (var move in movesToReverse)
            {
                // Reverse CABA entries
                // Note: You'll need to implement the reverse logic
            }
        }

        UpdateMatchingNumber(allReconciled);
    }

    public void UpdateMatchingNumber(IEnumerable<Account.AccountMoveLine> amls)
    {
        // Implementation of _update_matching_number method
        // Note: This is a simplified version. You'll need to adapt the logic to C#
    }

    public IDictionary<int, object> CollectTaxCashBasisValues()
    {
        // Implementation of _collect_tax_cash_basis_values method
        // Note: This is a simplified version. You'll need to adapt the logic to C#
        return new Dictionary<int, object>();
    }

    public IEnumerable<Account.AccountMove> CreateTaxCashBasisMoves()
    {
        // Implementation of _create_tax_cash_basis_moves method
        // Note: This is a simplified version. You'll need to adapt the logic to C#
        return new List<Account.AccountMove>();
    }
}
