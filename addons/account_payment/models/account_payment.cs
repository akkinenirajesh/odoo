csharp
public partial class AccountPayment
{
    public void ActionPost()
    {
        var paymentsNeedTx = this.Env.Query<AccountPayment>()
            .Where(p => p.PaymentTokenId != null && p.PaymentTransactionId == null)
            .ToList();

        var transactions = paymentsNeedTx.Select(p => p.CreatePaymentTransaction()).ToList();

        // Call base implementation for payments that don't need transactions
        base.ActionPost();

        foreach (var tx in transactions)
        {
            tx.SendPaymentRequest();
        }

        // Post-process transactions
        transactions.ForEach(tx => tx.PostProcess());

        var paymentsTxDone = paymentsNeedTx.Where(p => p.PaymentTransactionId.State == "done").ToList();
        paymentsTxDone.ForEach(p => p.ActionPost());

        var paymentsTxNotDone = paymentsNeedTx.Where(p => p.PaymentTransactionId.State != "done").ToList();
        paymentsTxNotDone.ForEach(p => p.ActionCancel());
    }

    public ActionResult ActionRefundWizard()
    {
        return new ActionResult
        {
            Name = "Refund",
            Type = ActionType.ActWindow,
            ViewMode = "form",
            ResModel = "Payment.RefundWizard",
            Target = "new"
        };
    }

    public ActionResult ActionViewRefunds()
    {
        var action = new ActionResult
        {
            Name = "Refund",
            ResModel = "Account.AccountPayment",
            Type = ActionType.ActWindow
        };

        if (this.RefundsCount == 1)
        {
            var refundTx = this.Env.Query<AccountPayment>()
                .FirstOrDefault(p => p.SourcePaymentId == this.Id);

            action.ResId = refundTx.Id;
            action.ViewMode = "form";
        }
        else
        {
            action.ViewMode = "tree,form";
            action.Domain = new Domain(("SourcePaymentId", "=", this.Id));
        }

        return action;
    }

    private PaymentTransaction CreatePaymentTransaction()
    {
        if (this.PaymentTransactionId != null)
        {
            throw new ValidationException($"A payment transaction with reference {this.PaymentTransactionId.Reference} already exists.");
        }

        if (this.PaymentTokenId == null)
        {
            throw new ValidationException("A token is required to create a new payment transaction.");
        }

        var transactionVals = this.PreparePaymentTransactionVals();
        var transaction = this.Env.Create<PaymentTransaction>(transactionVals);
        this.PaymentTransactionId = transaction;

        return transaction;
    }

    private Dictionary<string, object> PreparePaymentTransactionVals()
    {
        return new Dictionary<string, object>
        {
            ["ProviderId"] = this.PaymentTokenId.ProviderId.Id,
            ["PaymentMethodId"] = this.PaymentTokenId.PaymentMethodId.Id,
            ["Reference"] = this.Env.Get<PaymentTransaction>().ComputeReference(this.PaymentTokenId.ProviderId.Code, this.Ref),
            ["Amount"] = this.Amount,
            ["CurrencyId"] = this.CurrencyId.Id,
            ["PartnerId"] = this.PartnerId.Id,
            ["TokenId"] = this.PaymentTokenId.Id,
            ["Operation"] = "offline",
            ["PaymentId"] = this.Id
        };
    }

    public Dictionary<string, object> GetPaymentRefundWizardValues()
    {
        return new Dictionary<string, object>
        {
            ["TransactionId"] = this.PaymentTransactionId.Id,
            ["PaymentAmount"] = this.Amount,
            ["AmountAvailableForRefund"] = this.AmountAvailableForRefund
        };
    }
}
