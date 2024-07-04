csharp
public partial class SalePaymentTransaction {
    public void ComputeSaleOrderIdsNbr() {
        this.SaleOrderIdsNbr = this.SaleOrderIds.Count;
    }

    public void PostProcess() {
        foreach (var pendingTx in this.Where(tx => tx.State == "Pending")) {
            base.PostProcess(pendingTx);

            var salesOrders = pendingTx.SaleOrderIds.Where(so => so.State == "Draft" || so.State == "Sent");
            salesOrders.Where(so => so.State == "Draft").WithContext("tracking_disable", true).ActionQuotationSent();

            if (pendingTx.ProviderId.Code == "custom") {
                foreach (var order in pendingTx.SaleOrderIds) {
                    order.Reference = ComputeSaleOrderReference(order);
                }
            }

            if (pendingTx.Operation == "validation") {
                continue;
            }

            // Send the payment status email.
            salesOrders.Map(order => order.TransactionIds);
            salesOrders.SendPaymentSucceededForOrderMail();
        }

        foreach (var authorizedTx in this.Where(tx => tx.State == "Authorized")) {
            base.PostProcess(authorizedTx);
            var confirmedOrders = authorizedTx.CheckAmountAndConfirmOrder();
            confirmedOrders.SendOrderConfirmationMail();
            (this.SaleOrderIds - confirmedOrders).SendPaymentSucceededForOrderMail();
        }

        base.PostProcess(this.Where(tx => tx.State != "Pending" && tx.State != "Authorized" && tx.State != "Done"));

        foreach (var doneTx in this.Where(tx => tx.State == "Done")) {
            var confirmedOrders = doneTx.CheckAmountAndConfirmOrder();
            confirmedOrders.SendOrderConfirmationMail();
            (doneTx.SaleOrderIds - confirmedOrders).SendPaymentSucceededForOrderMail();

            var autoInvoice = Env.GetBool("sale.automatic_invoice");
            if (autoInvoice) {
                doneTx.InvoiceSaleOrders();
            }

            base.PostProcess(doneTx);

            if (autoInvoice) {
                this.SendInvoice();
            }
        }
    }

    private string ComputeSaleOrderReference(Sale.SaleOrder order) {
        if (this.ProviderId.SoReferenceType == "so_name") {
            return order.Name;
        } else {
            // this.ProviderId.SoReferenceType == "partner"
            var identificationNumber = order.PartnerId.Id;
            return $"CUST/{identificationNumber % 97:D2}";
        }
    }

    private Sale.SaleOrder CheckAmountAndConfirmOrder() {
        var confirmedOrders = new Sale.SaleOrder();
        foreach (var tx in this) {
            if (tx.SaleOrderIds.Count == 1) {
                var quotation = tx.SaleOrderIds.Where(so => so.State == "Draft" || so.State == "Sent");
                if (quotation && quotation.IsConfirmationAmountReached()) {
                    quotation.WithContext("send_email", true).ActionConfirm();
                    confirmedOrders |= quotation;
                }
            }
        }
        return confirmedOrders;
    }

    public void LogMessageOnLinkedDocuments(string message) {
        base.LogMessageOnLinkedDocuments(message);
        var author = Env.User.PartnerId;
        foreach (var order in this.SaleOrderIds.Union(this.SourceTransactionId.SaleOrderIds)) {
            order.MessagePost(message, author.Id);
        }
    }

    public void SendInvoice() {
        var templateId = Env.GetInt("sale.default_invoice_email_template", 0);
        if (templateId == 0) {
            return;
        }

        var template = Env.Get("mail.template").Browse(templateId);
        if (!template.Exists()) {
            return;
        }

        foreach (var tx in this) {
            tx = tx.WithCompany(tx.CompanyId);
            var invoiceToSend = tx.InvoiceIds.Where(i => !i.IsMoveSent && i.State == "posted" && i.IsReadyToBeSent());
            invoiceToSend.IsMoveSent = true;
            invoiceToSend.WithUser(Env.SuperuserId).GeneratePdfAndSendInvoice(template);
        }
    }

    public void CronSendInvoice() {
        if (!Env.GetBool("sale.automatic_invoice")) {
            return;
        }

        // Retrieve all transactions matching the criteria for post-processing
        this.Search(new[] {
            new Tuple<string, object>("State", "Done"),
            new Tuple<string, object>("IsPostProcessed", true),
            new Tuple<string, object>("InvoiceIds", Env.Get("account.move").Search(new[] {
                new Tuple<string, object>("IsMoveSent", false),
                new Tuple<string, object>("State", "posted"),
            })),
            new Tuple<string, object>("SaleOrderIds.State", "Sale"),
            new Tuple<string, object>("LastStateChange", DateTime.Now.AddDays(-2)),
        }).SendInvoice();
    }

    public void InvoiceSaleOrders() {
        foreach (var tx in this.Where(tx => tx.SaleOrderIds != null)) {
            tx = tx.WithCompany(tx.CompanyId);
            var confirmedOrders = tx.SaleOrderIds.Where(so => so.State == "Sale");
            if (confirmedOrders.Any()) {
                var fullyPaidOrders = confirmedOrders.Where(so => so.IsPaid());
                var downpaymentInvoices = (confirmedOrders - fullyPaidOrders).GenerateDownpaymentInvoices();
                var finalInvoices = fullyPaidOrders.WithContext("raise_if_nothing_to_invoice", false).CreateInvoices(true);
                var invoices = downpaymentInvoices.Union(finalInvoices);

                foreach (var invoice in invoices) {
                    invoice.EnsureToken();
                }

                tx.InvoiceIds = invoices.Select(invoice => invoice.Id).ToList();
            }
        }
    }

    private string ComputeReferencePrefix(string providerCode, string separator, params object[] values) {
        var commandList = values.FirstOrDefault(v => v is List<object>) as List<object>;
        if (commandList != null) {
            var orderIds = this.GetFieldValue("SaleOrderIds", commandList);
            var orders = Env.Get("sale.order").Browse(orderIds).Where(o => o.Exists());
            if (orders.Count() == orderIds.Count()) {
                return string.Join(separator, orders.Select(order => order.Name));
            }
        }

        return base.ComputeReferencePrefix(providerCode, separator, values);
    }

    public void ActionViewSalesOrders() {
        var action = new {
            Name = "Sales Order(s)",
            Type = "ir.actions.act_window",
            ResModel = "sale.order",
            Target = "current"
        };

        var saleOrderIds = this.SaleOrderIds.Select(so => so.Id).ToList();
        if (saleOrderIds.Count == 1) {
            action = action.With("ResId", saleOrderIds[0]);
            action = action.With("ViewMode", "form");
        } else {
            action = action.With("ViewMode", "tree,form");
            action = action.With("Domain", new[] { new Tuple<string, object>("Id", saleOrderIds) });
        }
        return action;
    }
}
