csharp
public partial class PaymentTransaction
{
    public int ComputeInvoicesCount()
    {
        if (this.Id == 0) return 0;

        var query = @"
            SELECT COUNT(invoice_id)
            FROM account_invoice_transaction_rel
            WHERE transaction_id = @transactionId
        ";

        using (var command = Env.Connection.CreateCommand())
        {
            command.CommandText = query;
            command.Parameters.AddWithValue("@transactionId", this.Id);
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    public ActionResult ActionViewInvoices()
    {
        var action = new ActionResult
        {
            Name = "Invoices",
            Type = "ir.actions.act_window",
            ResModel = "Account.Move",
            Target = "current"
        };

        var invoiceIds = this.InvoiceIds.Select(i => i.Id).ToList();
        if (invoiceIds.Count == 1)
        {
            action.ResId = invoiceIds[0];
            action.ViewMode = "form";
            action.Views = new List<(int, string)> { (Env.Ref("Account.ViewMoveForm").Id, "form") };
        }
        else
        {
            action.ViewMode = "tree,form";
            action.Domain = new List<object> { new List<object> { "Id", "in", invoiceIds } };
        }

        return action;
    }

    public string ComputeReferencePrefix(string providerCode, string separator, Dictionary<string, object> values)
    {
        if (values.TryGetValue("InvoiceIds", out var invoiceIdsObj) && invoiceIdsObj is List<int> invoiceIds)
        {
            var invoices = Env.Get<AccountMove>().Browse(invoiceIds);
            if (invoices.Count == invoiceIds.Count)
            {
                return string.Join(separator, invoices.Select(i => i.Name));
            }
        }
        return base.ComputeReferencePrefix(providerCode, separator, values);
    }

    public void PostProcess()
    {
        base.PostProcess();

        if (this.State == "done")
        {
            // Validate invoices automatically once the transaction is confirmed.
            foreach (var invoice in this.InvoiceIds.Where(inv => inv.State == "draft"))
            {
                invoice.ActionPost();
            }

            // Create and post missing payments.
            if (this.Operation != "validation" && this.PaymentId == null && 
                !this.ChildTransactionIds.Any(child => child.State == "done" || child.State == "cancel"))
            {
                using (var company = Env.WithCompany(this.CompanyId))
                {
                    this.CreatePayment();
                }
            }

            if (this.PaymentId != null)
            {
                var message = $"The payment related to the transaction with reference {this.Reference} has been posted: {this.PaymentId.GetHtmlLink()}";
                this.LogMessageOnLinkedDocuments(message);
            }
        }
        else if (this.State == "cancel")
        {
            this.PaymentId?.ActionCancel();
        }
    }

    public AccountPayment CreatePayment(Dictionary<string, object> extraCreateValues = null)
    {
        var reference = $"{this.Reference} - {this.PartnerId.DisplayName ?? ""} - {this.ProviderReference ?? ""}";

        var paymentMethodLine = this.ProviderId.JournalId.InboundPaymentMethodLineIds
            .FirstOrDefault(l => l.PaymentProviderId == this.ProviderId);

        var paymentValues = new Dictionary<string, object>
        {
            ["Amount"] = Math.Abs(this.Amount),
            ["PaymentType"] = this.Amount > 0 ? "inbound" : "outbound",
            ["CurrencyId"] = this.CurrencyId.Id,
            ["PartnerId"] = this.PartnerId.CommercialPartnerId.Id,
            ["PartnerType"] = "customer",
            ["JournalId"] = this.ProviderId.JournalId.Id,
            ["CompanyId"] = this.ProviderId.CompanyId.Id,
            ["PaymentMethodLineId"] = paymentMethodLine.Id,
            ["PaymentTokenId"] = this.TokenId?.Id,
            ["PaymentTransactionId"] = this.Id,
            ["Ref"] = reference
        };

        if (extraCreateValues != null)
        {
            foreach (var kvp in extraCreateValues)
            {
                paymentValues[kvp.Key] = kvp.Value;
            }
        }

        var payment = Env.Get<AccountPayment>().Create(paymentValues);
        payment.ActionPost();

        // Track the payment to make a one2one.
        this.PaymentId = payment;

        // Reconcile the payment with the source transaction's invoices in case of a partial capture.
        var invoices = this.Operation == this.SourceTransactionId?.Operation
            ? this.SourceTransactionId?.InvoiceIds
            : this.InvoiceIds;

        if (invoices != null && invoices.Any())
        {
            foreach (var invoice in invoices.Where(inv => inv.State == "draft"))
            {
                invoice.ActionPost();
            }

            var linesToReconcile = payment.LineIds.Concat(invoices.SelectMany(i => i.LineIds))
                .Where(line => line.AccountId == payment.DestinationAccountId && !line.Reconciled);

            Env.Get<AccountMoveLine>().Reconcile(linesToReconcile);
        }

        return payment;
    }

    public void LogMessageOnLinkedDocuments(string message)
    {
        var author = Env.Uid == 1 ? Env.User.PartnerId : this.PartnerId;

        if (this.SourceTransactionId != null)
        {
            foreach (var invoice in this.SourceTransactionId.InvoiceIds)
            {
                invoice.MessagePost(body: message, authorId: author.Id);
            }
            this.SourceTransactionId.PaymentId?.MessagePost(body: message, authorId: author.Id);
        }

        foreach (var invoice in this.InvoiceIds)
        {
            invoice.MessagePost(body: message, authorId: author.Id);
        }
    }
}
