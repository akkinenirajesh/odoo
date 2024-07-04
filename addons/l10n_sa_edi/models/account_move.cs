C#
public partial class AccountMove
{
    public bool L10nSaIsSimplified()
    {
        return Env.Ref<AccountMove>(this.Id).Partner.CompanyType == "person";
    }

    public void ComputeQrCodeStr()
    {
        if (this.CountryCode == "SA" && this.MoveType == "out_invoice" || this.MoveType == "out_refund")
        {
            var zatcaDocument = Env.Ref<AccountEdiXmlUbl21Zatca>(this.Id).EdiDocumentIds.Where(d => d.EdiFormatId.Code == "sa_zatca").FirstOrDefault();
            if (zatcaDocument != null && this.State != "draft")
            {
                string qrCodeStr = "";
                if (this.L10nSaIsSimplified())
                {
                    var x509Cert = Env.Ref<AccountJournal>(this.JournalId).L10nSaProductionCsidJson;
                    var xmlContent = Env.Ref<AccountEdiXmlUbl21Zatca>(this.Id).L10nSaGenerateZatcaTemplate(this);
                    qrCodeStr = this.L10nSaGetQrCode(this.JournalId, xmlContent, x509Cert, this.L10nSaInvoiceSignature, true);
                    qrCodeStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(qrCodeStr));
                }
                else if (zatcaDocument.State == "sent" && zatcaDocument.AttachmentId.Datas != null)
                {
                    var documentXml = Encoding.UTF8.GetString(zatcaDocument.AttachmentId.Datas);
                    var root = XDocument.Parse(documentXml);
                    var qrNode = root.XPathSelectElement("//*[local-name()='ID'][text()='QR']/following-sibling::*/*");
                    qrCodeStr = qrNode.Value;
                }
                this.L10nSaQrCodeStr = qrCodeStr;
            }
        }
    }

    public string L10nSaGetQrCodeEncoding(int tag, string field, int intLength = 1)
    {
        var companyNameTagEncoding = BitConverter.GetBytes(tag);
        var companyNameLengthEncoding = BitConverter.GetBytes(field.Length);
        return companyNameTagEncoding.Concat(companyNameLengthEncoding).Concat(Encoding.UTF8.GetBytes(field)).ToArray();
    }

    public string L10nSaGetQrCode(int journalId, string unsignedXml, string x509Cert, string signature, bool isB2c = false)
    {
        var root = XDocument.Parse(unsignedXml);
        var ediFormat = Env.Ref<AccountEdiXmlUbl21Zatca>(this.Id);
        var invoiceDate = root.XPathSelectElement("//cbc:IssueDate").Value;
        var invoiceTime = root.XPathSelectElement("//cbc:IssueTime").Value;
        var invoiceDatetime = DateTime.ParseExact(invoiceDate + " " + invoiceTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        if (invoiceDatetime != null && Env.Ref<AccountJournal>(journalId).Company.Vat != null && x509Cert != null)
        {
            var prehashContent = root.ToString();
            var invoiceHash = ediFormat.L10nSaGenerateInvoiceXmlHash(prehashContent, "digest");
            var amountTotal = Convert.ToDecimal(root.XPathSelectElement("//cbc:TaxInclusiveAmount").Value);
            var amountTax = Convert.ToDecimal(root.XPathSelectElement("//cac:TaxTotal/cbc:TaxAmount").Value);
            var x509Certificate = new X509Certificate2(Convert.FromBase64String(x509Cert));
            var sellerNameEnc = this.L10nSaGetQrCodeEncoding(1, Env.Ref<AccountJournal>(journalId).Company.DisplayName);
            var sellerVatEnc = this.L10nSaGetQrCodeEncoding(2, Env.Ref<AccountJournal>(journalId).Company.Vat);
            var timestampEnc = this.L10nSaGetQrCodeEncoding(3, invoiceDatetime.ToString("yyyy-MM-ddTHH:mm:ss"));
            var amountTotalEnc = this.L10nSaGetQrCodeEncoding(4, amountTotal.ToString("0.00"));
            var amountTaxEnc = this.L10nSaGetQrCodeEncoding(5, amountTax.ToString("0.00"));
            var invoiceHashEnc = this.L10nSaGetQrCodeEncoding(6, invoiceHash);
            var signatureEnc = this.L10nSaGetQrCodeEncoding(7, signature);
            var publicKeyEnc = this.L10nSaGetQrCodeEncoding(8, Convert.ToBase64String(x509Certificate.PublicKey.ExportSubjectPublicKeyInfo()));
            var qrCodeStr = sellerNameEnc.Concat(sellerVatEnc).Concat(timestampEnc).Concat(amountTotalEnc).Concat(amountTaxEnc).Concat(invoiceHashEnc).Concat(signatureEnc).Concat(publicKeyEnc).ToArray();
            if (isB2c)
            {
                qrCodeStr = qrCodeStr.Concat(this.L10nSaGetQrCodeEncoding(9, x509Certificate.Signature));
            }
            return Encoding.UTF8.GetString(qrCodeStr);
        }
        return null;
    }

    public void ComputeEdiShowCancelButton()
    {
        if (this.IsInvoice() && this.CountryCode == "SA")
        {
            this.EdiShowCancelButton = false;
        }
    }

    public void ComputeShowResetToDraftButton()
    {
        if (this.L10nSaChainIndex != null)
        {
            this.ShowResetToDraftButton = false;
        }
    }

    public string L10nSaGenerateUnsignedData()
    {
        var ediFormat = Env.Ref<AccountEdiXmlUbl21Zatca>(this.Id);
        this.L10nSaUuid = Guid.NewGuid().ToString();
        var xmlContent = ediFormat.L10nSaGenerateZatcaTemplate(this);
        var invoiceHashHex = ediFormat.L10nSaGenerateInvoiceXmlHash(xmlContent).ToString();
        this.L10nSaInvoiceSignature = ediFormat.L10nSaGetDigitalSignature(this.JournalId.Company, invoiceHashHex);
        return xmlContent;
    }

    public void L10nSaLogResults(string xmlContent, string responseData, bool error)
    {
        this.JournalId.L10nSaLatestSubmissionHash = Env.Ref<AccountEdiXmlUbl21Zatca>(this.Id).L10nSaGenerateInvoiceXmlHash(xmlContent);
        if (error)
        {
            this.MessagePost("Invoice was rejected by ZATCA. Please, check the response below:", responseData);
        }
        else if (responseData != null && !string.IsNullOrEmpty(responseData))
        {
            this.MessagePost("Invoice was Accepted by ZATCA (with Warnings). Please, check the response below:", responseData);
        }
    }

    public bool L10nSaIsInChain()
    {
        var zatcaDocIds = Env.Ref<AccountEdiXmlUbl21Zatca>(this.Id).EdiDocumentIds.Where(d => d.EdiFormatId.Code == "sa_zatca");
        return zatcaDocIds != null && zatcaDocIds.Any() && !zatcaDocIds.Any(d => d.State == "to_send");
    }
}
