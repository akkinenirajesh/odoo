csharp
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

public partial class ThumbDrive
{
    public string ActionSignInvoices(List<int> invoiceIds)
    {
        string signHost = GetHost();

        var toSignDict = new Dictionary<int, string>();
        foreach (var invoiceId in invoiceIds)
        {
            var invoice = Env.Get<AccountMove>().Browse(invoiceId);
            var etaInvoice = JsonSerializer.Deserialize<Dictionary<string, object>>(invoice.L10nEgEtaJsonDocId.Raw);
            var signedAttrs = GenerateSignedAttrs((Dictionary<string, object>)etaInvoice["request"], invoice.L10nEgSigningTime);
            toSignDict[invoiceId] = Convert.ToBase64String(signedAttrs);
        }

        return JsonSerializer.Serialize(new
        {
            type = "ir.actions.client",
            tag = "action_post_sign_invoice",
            @params = new
            {
                sign_host = signHost,
                access_token = AccessToken,
                pin = Pin,
                drive_id = Id,
                invoices = JsonSerializer.Serialize(toSignDict)
            }
        });
    }

    public string ActionSetCertificateFromUsb()
    {
        string signHost = GetHost();

        return JsonSerializer.Serialize(new
        {
            type = "ir.actions.client",
            tag = "action_get_drive_certificate",
            @params = new
            {
                sign_host = signHost,
                access_token = AccessToken,
                pin = Pin,
                drive_id = Id
            }
        });
    }

    public bool SetCertificate(string certificate)
    {
        Certificate = Encoding.UTF8.GetBytes(certificate);
        return true;
    }

    public bool SetSignatureData(string invoices)
    {
        var invoicesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(invoices);
        foreach (var kvp in invoicesDict)
        {
            int invoiceId = int.Parse(kvp.Key);
            var invoice = Env.Get<AccountMove>().Browse(invoiceId);
            var etaInvoiceJson = JsonSerializer.Deserialize<Dictionary<string, object>>(invoice.L10nEgEtaJsonDocId.Raw);

            string signature = GenerateCadesBesSignature(
                (Dictionary<string, object>)etaInvoiceJson["request"],
                invoice.L10nEgSigningTime,
                Convert.FromBase64String(kvp.Value)
            );

            ((Dictionary<string, object>)etaInvoiceJson["request"])["signatures"] = new[]
            {
                new Dictionary<string, object>
                {
                    { "signatureType", "I" },
                    { "value", signature }
                }
            };

            invoice.L10nEgEtaJsonDocId.Raw = JsonSerializer.Serialize(etaInvoiceJson);
            invoice.L10nEgIsSigned = true;
        }
        return true;
    }

    private string GetHost()
    {
        string signHost = Env.Get<IrConfigParameter>().GetParam("l10n_eg_eta.sign.host", "http://localhost:8069");
        if (string.IsNullOrEmpty(signHost))
        {
            throw new ValidationException("Please define the host of sign tool.");
        }
        return signHost;
    }

    private byte[] GenerateSignedAttrs(Dictionary<string, object> etaInvoice, DateTime signingTime)
    {
        // Implementation of _generate_signed_attrs method
        // This would require more complex logic to replicate the exact behavior
        throw new NotImplementedException();
    }

    private string GenerateCadesBesSignature(Dictionary<string, object> etaInvoice, DateTime signingTime, byte[] signature)
    {
        // Implementation of _generate_cades_bes_signature method
        // This would require more complex logic to replicate the exact behavior
        throw new NotImplementedException();
    }
}
