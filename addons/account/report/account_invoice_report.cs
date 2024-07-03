csharp
public partial class AccountInvoiceReport
{
    public override string ToString()
    {
        return InvoiceDate.ToString();
    }

    public SQL TableQuery()
    {
        return SQL.Combine(Select(), From(), Where());
    }

    private SQL Select()
    {
        // Implementation of _select method
        // This should return an SQL object representing the SELECT part of the query
        throw new NotImplementedException();
    }

    private SQL From()
    {
        // Implementation of _from method
        // This should return an SQL object representing the FROM part of the query
        throw new NotImplementedException();
    }

    private SQL Where()
    {
        // Implementation of _where method
        // This should return an SQL object representing the WHERE part of the query
        throw new NotImplementedException();
    }
}

public partial class ReportInvoiceWithoutPayment
{
    public Dictionary<string, object> GetReportValues(List<int> docIds, Dictionary<string, object> data = null)
    {
        var docs = Env.Get<AccountMove>().Browse(docIds);

        var qrCodeUrls = new Dictionary<int, string>();
        foreach (var invoice in docs)
        {
            if (invoice.DisplayQrCode)
            {
                var newCodeUrl = invoice.GenerateQrCode(silentErrors: data?["ReportType"]?.ToString() == "html");
                if (!string.IsNullOrEmpty(newCodeUrl))
                {
                    qrCodeUrls[invoice.Id] = newCodeUrl;
                }
            }
        }

        return new Dictionary<string, object>
        {
            ["DocIds"] = docIds,
            ["DocModel"] = "Account.AccountMove",
            ["Docs"] = docs,
            ["QrCodeUrls"] = qrCodeUrls
        };
    }
}

public partial class ReportInvoiceWithPayment : ReportInvoiceWithoutPayment
{
    public new Dictionary<string, object> GetReportValues(List<int> docIds, Dictionary<string, object> data = null)
    {
        var result = base.GetReportValues(docIds, data);
        result["ReportType"] = data?["ReportType"] ?? "";
        return result;
    }
}
