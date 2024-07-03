csharp
public partial class AccountMove
{
    public List<Payment.Transaction> ComputeAuthorizedTransactionIds()
    {
        return TransactionIds.Where(tx => tx.State == "authorized").ToList();
    }

    public int ComputeTransactionCount()
    {
        return TransactionIds.Count;
    }

    public decimal ComputeAmountPaid()
    {
        return TransactionIds
            .Where(tx => tx.State == "authorized" || tx.State == "done")
            .Sum(tx => tx.Amount);
    }

    public bool HasToBePaid()
    {
        var transactions = TransactionIds.Where(tx => new[] { "pending", "authorized", "done" }.Contains(tx.State));
        var pendingTransactions = transactions.Where(tx => 
            new[] { "pending", "authorized" }.Contains(tx.State) && 
            !new[] { "none", "custom" }.Contains(tx.ProviderCode));

        var enabledFeature = Env.GetParam<bool>("account_payment.enable_portal_payment");

        return enabledFeature && 
               (AmountResidual != 0 || !transactions.Any()) &&
               State == "posted" &&
               new[] { "not_paid", "partial" }.Contains(PaymentState) &&
               AmountTotal != 0 &&
               MoveType == "out_invoice" &&
               !pendingTransactions.Any();
    }

    public Payment.Transaction GetPortalLastTransaction()
    {
        return Env.WithContext(new { active_test = false })
            .Get<List<Payment.Transaction>>("TransactionIds")
            .OrderByDescending(tx => tx.Id)
            .FirstOrDefault();
    }

    public void PaymentActionCapture()
    {
        PaymentUtils.CheckRightsOnRecordset(this);
        Env.Sudo().Get<List<Payment.Transaction>>("TransactionIds").ActionCapture();
    }

    public void PaymentActionVoid()
    {
        PaymentUtils.CheckRightsOnRecordset(this);
        Env.Sudo().Get<List<Payment.Transaction>>("AuthorizedTransactionIds").ActionVoid();
    }

    public Dictionary<string, object> ActionViewPaymentTransactions()
    {
        var action = Env.Get<IrActionsWindow>().ForXmlId("payment.action_payment_transaction");

        if (TransactionIds.Count == 1)
        {
            action["view_mode"] = "form";
            action["res_id"] = TransactionIds[0].Id;
            action["views"] = new List<object>();
        }
        else
        {
            action["domain"] = new List<object> { new List<object> { "id", "in", TransactionIds.Select(t => t.Id).ToList() } };
        }

        return action;
    }

    public Dictionary<string, object> GetDefaultPaymentLinkValues()
    {
        return new Dictionary<string, object>
        {
            { "amount", AmountResidual },
            { "currency_id", CurrencyId },
            { "partner_id", PartnerId },
            { "amount_max", AmountResidual }
        };
    }
}
