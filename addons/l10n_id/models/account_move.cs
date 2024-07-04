csharp
public partial class AccountMove
{
    public string GenerateQrCode(bool silentErrors = false)
    {
        // Implement the QR code generation logic here
        // You might need to call a base implementation or use a different approach
        return "";
    }

    public void L10nIdCronUpdatePaymentStatus()
    {
        var invoices = Env.Search<AccountMove>(new[]
        {
            ("PaymentState", "=", "not_paid"),
            ("L10nIdQrisInvoiceDetails", "!=", null)
        });
        invoices.L10nIdUpdatePaymentStatus();
    }

    public void ActionL10nIdUpdatePaymentStatus()
    {
        var invoices = this.FilteredDomain(new[]
        {
            ("PaymentState", "=", "not_paid"),
            ("L10nIdQrisInvoiceDetails", "!=", null)
        });
        invoices.L10nIdUpdatePaymentStatus();
    }

    private void L10nIdUpdatePaymentStatus()
    {
        var qrStatuses = L10nIdGetQrisQrStatuses();
        L10nIdProcessInvoices(qrStatuses);
    }

    private Dictionary<long, QrStatus> L10nIdGetQrisQrStatuses()
    {
        var result = new Dictionary<long, QrStatus>();
        foreach (var invoice in this)
        {
            bool paid = false;
            var unpaidData = new List<Dictionary<string, object>>();
            var paidData = new List<Dictionary<string, object>>();

            foreach (var qrInvoice in invoice.L10nIdQrisInvoiceDetails.Reverse())
            {
                var statusResponse = invoice.PartnerBankId.L10nIdQrisFetchStatus(qrInvoice);
                if (statusResponse["data"].GetValueOrDefault("qris_status") as string == "paid")
                {
                    paidData.Add(statusResponse["data"] as Dictionary<string, object>);
                    paid = true;
                    break;
                }
                else
                {
                    unpaidData.Add(statusResponse["data"] as Dictionary<string, object>);
                }
            }

            result[invoice.Id] = new QrStatus
            {
                Paid = paid,
                QrStatuses = paid ? paidData : unpaidData
            };
        }
        return result;
    }

    private void L10nIdProcessInvoices(Dictionary<long, QrStatus> invoicesStatuses)
    {
        var jakartaNow = Env.Context.Now().ToJakartaTimeZone();
        var paidInvoices = new List<AccountMove>();
        var paidMessages = new Dictionary<long, string>();

        foreach (var invoice in this)
        {
            var statuses = invoicesStatuses[invoice.Id];
            if (statuses.Paid)
            {
                var paidStatus = statuses.QrStatuses[0];
                string message;
                if (paidStatus.ContainsKey("qris_payment_customername") && paidStatus.ContainsKey("qris_payment_methodby"))
                {
                    message = string.Format("This invoice was paid by {0} using QRIS with the payment method {1}.",
                        paidStatus["qris_payment_customername"],
                        paidStatus["qris_payment_methodby"]);
                }
                else
                {
                    message = "This invoice was paid using QRIS.";
                }
                paidInvoices.Add(invoice);
                paidMessages[invoice.Id] = message;
            }
            else
            {
                var qrisDataToRecheck = new List<Dictionary<string, object>>();
                foreach (var qrInvoice in invoice.L10nIdQrisInvoiceDetails)
                {
                    var qrisDateTime = DateTime.Parse(qrInvoice["qris_creation_datetime"] as string).ToJakartaTimeZone();
                    if ((jakartaNow - qrisDateTime).TotalSeconds < 1800)
                    {
                        qrisDataToRecheck.Add(qrInvoice);
                    }
                }
                invoice.L10nIdQrisInvoiceDetails = qrisDataToRecheck;
            }
        }

        if (paidInvoices.Any())
        {
            paidInvoices.MessageLogBatch(paidMessages);
            foreach (var invoice in paidInvoices)
            {
                invoice.L10nIdQrisInvoiceDetails = null;
            }

            // Register the payment
            var paymentRegister = Env.New<AccountPaymentRegister>(new
            {
                ActiveModel = "Account.AccountMove",
                ActiveIds = paidInvoices.Select(i => i.Id).ToList(),
                GroupPayment = false
            });
            paymentRegister.ActionCreatePayments();
        }
    }
}

public class QrStatus
{
    public bool Paid { get; set; }
    public List<Dictionary<string, object>> QrStatuses { get; set; }
}
