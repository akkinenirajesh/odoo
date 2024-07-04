csharp
public partial class AccountEdiFormat
{
    public string GetL10nInEdiXmlInvoiceContent(AccountMove invoice)
    {
        var jsonContent = GenerateL10nInEdiInvoiceJson(invoice);
        return System.Text.Json.JsonSerializer.Serialize(jsonContent);
    }

    public Dictionary<string, object> L10nInEdiPostInvoice(AccountMove invoice)
    {
        var generateJson = GenerateL10nInEdiInvoiceJson(invoice);
        var response = L10nInEdiGenerate(invoice.Company, generateJson);

        if (response.ContainsKey("error"))
        {
            var error = response["error"] as List<Dictionary<string, object>>;
            var errorCodes = error.Select(e => e["code"].ToString()).ToList();

            if (errorCodes.Contains("1005"))
            {
                var authenticateResponse = L10nInEdiAuthenticate(invoice.Company);
                if (!authenticateResponse.ContainsKey("error"))
                {
                    error.Clear();
                    response = L10nInEdiGenerate(invoice.Company, generateJson);
                    if (response.ContainsKey("error"))
                    {
                        error = response["error"] as List<Dictionary<string, object>>;
                        errorCodes = error.Select(e => e["code"].ToString()).ToList();
                    }
                }
            }

            if (errorCodes.Contains("2150"))
            {
                response = L10nInEdiGetIrnByDetails(invoice.Company, new Dictionary<string, object>
                {
                    {"doc_type", invoice.MoveType == "out_refund" ? "CRN" : "INV"},
                    {"doc_num", invoice.Name},
                    {"doc_date", invoice.InvoiceDate?.ToString("dd/MM/yyyy")}
                });

                if (!response.ContainsKey("error"))
                {
                    error.Clear();
                    // Post message about invoice already submitted
                }
            }

            if (errorCodes.Contains("no-credit"))
            {
                return new Dictionary<string, object>
                {
                    {"success", false},
                    {"error", GetL10nInEdiIapBuyCreditsMessage(invoice.Company)},
                    {"blocking_level", "error"}
                };
            }
            else if (error.Any())
            {
                var errorMessage = string.Join("<br/>", error.Select(e => $"[{e["code"]}] {e["message"]}"));
                return new Dictionary<string, object>
                {
                    {"success", false},
                    {"error", errorMessage},
                    {"blocking_level", errorCodes.Contains("404") ? "warning" : "error"}
                };
            }
        }

        if (!response.ContainsKey("error"))
        {
            var jsonDump = System.Text.Json.JsonSerializer.Serialize(response["data"]);
            var jsonName = $"{invoice.Name.Replace("/", "_")}_einvoice.json";
            
            // Create attachment logic here

            return new Dictionary<string, object>
            {
                {"success", true},
                {"attachment", null} // Replace with actual attachment object
            };
        }

        return new Dictionary<string, object>();
    }

    // Implement other methods like L10nInEdiCancelInvoice, GenerateL10nInEdiInvoiceJson, etc.
}
