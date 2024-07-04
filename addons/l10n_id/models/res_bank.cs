csharp
public partial class Bank
{
    public List<(string Method, string Name, int Priority)> GetAvailableQrMethods()
    {
        var result = new List<(string, string, int)>();
        // Add existing methods here
        result.Add(("id_qr", "QRIS", 40));
        return result;
    }

    public string GetErrorMessagesForQr(string qrMethod, ResPartner debtorPartner, Core.Currency currency)
    {
        if (qrMethod == "id_qr")
        {
            if (CountryCode != "ID")
            {
                return "You cannot generate a QRIS QR code with a bank account that is not in Indonesia.";
            }
            if (currency.Name != "IDR")
            {
                return "You cannot generate a QRIS QR code with a currency other than IDR";
            }
            if (string.IsNullOrEmpty(L10nIdQrisApiKey) || string.IsNullOrEmpty(L10nIdQrisMid))
            {
                return "To use QRIS QR code, Please setup the QRIS API Key and Merchant ID on the bank's configuration";
            }
            return null;
        }

        // Handle other methods here
        return null;
    }

    public string CheckForQrCodeErrors(string qrMethod, decimal amount, Core.Currency currency, ResPartner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "id_qr")
        {
            if (amount == 0)
            {
                return "The amount must be set to generate a QR code.";
            }
        }

        // Handle other methods here
        return null;
    }

    public string GetQrVals(string qrMethod, decimal amount, Core.Currency currency, ResPartner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "id_qr")
        {
            var invoice = Env.Get<Account.Move>().Browse(Env.Context.GetValueOrDefault("qris_originating_invoice_id") as int?);

            if (invoice != null && invoice.L10nIdQrisInvoiceDetails != null && invoice.L10nIdQrisInvoiceDetails.Any())
            {
                var now = DateTime.Now;
                var latestQrDate = invoice.L10nIdQrisInvoiceDetails.Last().QrisCreationDatetime;

                if ((now - latestQrDate).TotalSeconds < 1500)
                {
                    return invoice.L10nIdQrisInvoiceDetails.Last().QrisContent;
                }
            }

            var parameters = new Dictionary<string, string>
            {
                ["do"] = "create-invoice",
                ["apikey"] = L10nIdQrisApiKey,
                ["mID"] = L10nIdQrisMid,
                ["cliTrxNumber"] = structuredCommunication,
                ["cliTrxAmount"] = ((int)amount).ToString()
            };

            var response = L10nIdMakeQrisRequest("show_qris.php", parameters);
            var data = response.Data;

            if (invoice != null)
            {
                var qrisInvoiceDetails = invoice.L10nIdQrisInvoiceDetails ?? new List<QrisInvoiceDetail>();
                qrisInvoiceDetails.Add(new QrisInvoiceDetail
                {
                    QrisInvoiceId = data.QrisInvoiceid,
                    QrisAmount = (int)amount,
                    QrisCreationDatetime = data.QrisRequestDate,
                    QrisContent = data.QrisContent
                });
                invoice.L10nIdQrisInvoiceDetails = qrisInvoiceDetails;
            }

            return data.QrisContent;
        }

        // Handle other methods here
        return null;
    }

    public Dictionary<string, object> GetQrCodeGenerationParams(string qrMethod, decimal amount, Core.Currency currency, ResPartner debtorPartner, string freeCommunication, string structuredCommunication)
    {
        if (qrMethod == "id_qr")
        {
            if (!Env.Context.GetValueOrDefault("is_online_qr", false))
            {
                return new Dictionary<string, object>();
            }
            return new Dictionary<string, object>
            {
                ["barcode_type"] = "QR",
                ["width"] = 120,
                ["height"] = 120,
                ["value"] = GetQrVals(qrMethod, amount, currency, debtorPartner, freeCommunication, structuredCommunication)
            };
        }

        // Handle other methods here
        return new Dictionary<string, object>();
    }

    public Dictionary<string, object> L10nIdQrisFetchStatus(Dictionary<string, object> qrData)
    {
        return L10nIdMakeQrisRequest("checkpaid_qris.php", new Dictionary<string, string>
        {
            ["do"] = "checkStatus",
            ["apikey"] = L10nIdQrisApiKey,
            ["mID"] = L10nIdQrisMid,
            ["invid"] = qrData["qris_invoice_id"] as string,
            ["trxvalue"] = qrData["qris_amount"].ToString(),
            ["trxdate"] = qrData["qris_creation_datetime"] as string
        });
    }

    private Dictionary<string, object> L10nIdMakeQrisRequest(string endpoint, Dictionary<string, string> parameters)
    {
        // Implementation of API request to QRIS
        // This would typically use HttpClient or similar to make the actual request
        // and handle exceptions, timeouts, etc.
        throw new NotImplementedException();
    }
}
