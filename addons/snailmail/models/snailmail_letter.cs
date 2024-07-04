csharp
public partial class SnailmailLetter
{
    public SnailmailLetter() { }

    public int UserId { get; set; }
    public string Model { get; set; }
    public int ResId { get; set; }
    public int PartnerId { get; set; }
    public int CompanyId { get; set; }
    public int ReportTemplate { get; set; }
    public int AttachmentId { get; set; }
    public byte[] AttachmentDatas { get; set; }
    public string AttachmentFname { get; set; }
    public bool Color { get; set; }
    public bool Cover { get; set; }
    public bool Duplex { get; set; }
    public string State { get; set; }
    public string ErrorCode { get; set; }
    public string InfoMsg { get; set; }
    public string Reference { get; set; }
    public int MessageId { get; set; }
    public List<int> NotificationIds { get; set; }
    public string Street { get; set; }
    public string Street2 { get; set; }
    public string Zip { get; set; }
    public string City { get; set; }
    public int StateId { get; set; }
    public int CountryId { get; set; }

    public void ComputeReference()
    {
        this.Reference = string.Format("{0},{1}", this.Model, this.ResId);
    }

    public static List<string> States { get; } = new List<string>() { "pending", "sent", "error", "canceled" };
    public static List<string> ErrorCodes { get; } = new List<string>() { "MISSING_REQUIRED_FIELDS", "CREDIT_ERROR", "TRIAL_ERROR", "NO_PRICE_AVAILABLE", "FORMAT_ERROR", "UNKNOWN_ERROR", "ATTACHMENT_ERROR" };

    public void Create()
    {
        // this.Env[this.Model].Browse(this.ResId).MessagePost(
        //     body: this.Env.Translate("Letter sent by post with Snailmail"),
        //     message_type: "snailmail"
        // );

        // var partner = this.Env["res.partner"].Browse(this.PartnerId);
        // this.Street = partner.Street;
        // this.Street2 = partner.Street2;
        // this.Zip = partner.Zip;
        // this.City = partner.City;
        // this.StateId = partner.StateId;
        // this.CountryId = partner.CountryId;
    }

    public void Write()
    {
        // if (this.AttachmentId != 0)
        // {
        //     this.Env["ir.attachment"].Browse(this.AttachmentId).Check("read");
        // }
    }

    public void FetchAttachment()
    {
        // if (this.AttachmentId == 0)
        // {
        //     var obj = this.Env[this.Model].Browse(this.ResId);
        //     var report = this.ReportTemplate;

        //     if (report == 0)
        //     {
        //         var reportName = this.Env.Context.Get("report_name");
        //         report = this.Env["ir.actions.report"]._GetReportFromName(reportName);
        //         if (report == 0)
        //         {
        //             return;
        //         }
        //         else
        //         {
        //             this.ReportTemplate = report.Id;
        //         }
        //     }
        //     if (report.PrintReportName != null)
        //     {
        //         // reportName = safe_eval(report.PrintReportName, {'object': obj});
        //     }
        //     else if (report.Attachment != null)
        //     {
        //         // reportName = safe_eval(report.Attachment, {'object': obj});
        //     }
        //     else
        //     {
        //         var reportName = "Document";
        //     }

        //     var filename = string.Format("{0}.{1}", reportName, "pdf");
        //     var paperformat = report.GetPaperformat();
        //     if ((paperformat.Format == "custom" && paperformat.PageWidth != 210 && paperformat.PageHeight != 297) || paperformat.Format != "A4")
        //     {
        //         throw new UserError(this.Env.Translate("Please use an A4 Paper format."));
        //     }

        //     // var pdfBin = this.Env["ir.actions.report"].with_context(snailmail_layout: !this.Cover, lang: "en_US")._render_qweb_pdf(report, this.ResId);
        //     // pdfBin = this._OverwriteMargins(pdfBin);
        //     // if (this.Cover)
        //     // {
        //     //     pdfBin = this._AppendCoverPage(pdfBin);
        //     // }

        //     // this.AttachmentId = this.Env["ir.attachment"].Create(new Dictionary<string, object>() {
        //     //     { "Name", filename },
        //     //     { "Datas", base64.b64encode(pdfBin) },
        //     //     { "ResModel", "snailmail.letter" },
        //     //     { "ResId", this.Id },
        //     //     { "Type", "binary" }
        //     // });
        // }
    }

    // public byte[] _AppendCoverPage(byte[] invoiceBin)
    // {
    //     var outWriter = new PdfFileWriter();
    //     // var addressSplit = this.PartnerId.with_context(show_address: true, lang: "en_US").display_name.split("\n");
    //     // addressSplit[0] = this.PartnerId.Name ?? (this.PartnerId.ParentId != 0 ? this.PartnerId.ParentId.Name : addressSplit[0]);
    //     // var address = string.Join("<br/>", addressSplit);
    //     // var addressX = 118 * mm;
    //     // var addressY = 60 * mm;
    //     // var frameWidth = 85.5 * mm;
    //     // var frameHeight = 25.5 * mm;

    //     // var coverBuf = new MemoryStream();
    //     // var canvas = new Canvas(coverBuf, pagesize: A4);
    //     // var styles = getSampleStyleSheet();

    //     // var frame = new Frame(addressX, A4[1] - addressY - frameHeight, frameWidth, frameHeight);
    //     // var story = new List<Paragraph> { new Paragraph(address, styles["Normal"]) };
    //     // var addressInframe = new KeepInFrame(0, 0, story);
    //     // frame.addFromList(new List<KeepInFrame> { addressInframe }, canvas);
    //     // canvas.save();
    //     // coverBuf.Seek(0);

    //     // var invoice = new PdfFileReader(new MemoryStream(invoiceBin));
    //     // var coverBin = new MemoryStream(coverBuf.ToArray());
    //     // var coverFile = new PdfFileReader(coverBin);
    //     // outWriter.appendPagesFromReader(coverFile);

    //     // if (this.Duplex)
    //     // {
    //     //     outWriter.addBlankPage();
    //     // }

    //     // outWriter.appendPagesFromReader(invoice);

    //     // var outBuff = new MemoryStream();
    //     // outWriter.write(outBuff);
    //     // return outBuff.ToArray();
    // }

    // public byte[] _OverwriteMargins(byte[] invoiceBin)
    // {
    //     // var pdfBuf = new MemoryStream();
    //     // var canvas = new Canvas(pdfBuf, pagesize: A4);
    //     // canvas.setFillColorRGB(255, 255, 255);
    //     // var pageWidth = A4[0];
    //     // var pageHeight = A4[1];

    //     // var hmarginWidth = pageWidth;
    //     // var hmarginHeight = 5 * mm;

    //     // var vmarginWidth = 5 * mm;
    //     // var vmarginHeight = pageHeight;

    //     // var sqWidth = 15 * mm;

    //     // canvas.rect(0, 0, hmarginWidth, hmarginHeight, stroke: 0, fill: 1);
    //     // canvas.rect(0, pageHeight, hmarginWidth, -hmarginHeight, stroke: 0, fill: 1);

    //     // canvas.rect(0, 0, vmarginWidth, vmarginHeight, stroke: 0, fill: 1);
    //     // canvas.rect(pageWidth, 0, -vmarginWidth, vmarginHeight, stroke: 0, fill: 1);

    //     // canvas.rect(0, 0, sqWidth, sqWidth, stroke: 0, fill: 1);
    //     // canvas.save();
    //     // pdfBuf.Seek(0);

    //     // var newPdf = new PdfFileReader(pdfBuf);
    //     // var currPdf = new PdfFileReader(new MemoryStream(invoiceBin));
    //     // var out = new PdfFileWriter();
    //     // foreach (var page in currPdf.pages)
    //     // {
    //     //     page.mergePage(newPdf.getPage(0));
    //     //     out.addPage(page);
    //     // }
    //     // var outStream = new MemoryStream();
    //     // out.write(outStream);
    //     // var outBin = outStream.ToArray();
    //     // outStream.Close();
    //     // return outBin;
    // }

    public void SnailmailPrint()
    {
        // this.State = "pending";
        // this.NotificationIds.ForEach(id => {
        //     this.Env["mail.notification"].Browse(id).Write(new Dictionary<string, object>() {
        //         { "NotificationStatus", "ready" }
        //     });
        // });
        // if (this.NotificationIds.Count == 1)
        // {
        //     this._SnailmailPrint();
        // }
    }

    public void Cancel()
    {
        // this.State = "canceled";
        // this.ErrorCode = null;
        // this.NotificationIds.ForEach(id => {
        //     this.Env["mail.notification"].Browse(id).Write(new Dictionary<string, object>() {
        //         { "NotificationStatus", "canceled" }
        //     });
        // });
    }

    // public void _SnailmailCron()
    // {
    //     var letters = this.Search([
    //         "|",
    //         ("State", "=", "pending"),
    //         "&",
    //         ("State", "=", "error"),
    //         ("ErrorCode", "in", new List<string>() { "TRIAL_ERROR", "CREDIT_ERROR", "ATTACHMENT_ERROR", "MISSING_REQUIRED_FIELDS" })
    //     ]);
    //     foreach (var letter in letters)
    //     {
    //         letter._SnailmailPrint();
    //         if (letter.ErrorCode == "CREDIT_ERROR")
    //         {
    //             break;
    //         }
    //         this.Env.Cr.Commit();
    //     }
    // }

    // public bool _IsValidAddress()
    // {
    //     var requiredKeys = new List<string>() { "Street", "City", "Zip", "CountryId" };
    //     return requiredKeys.All(key => this[key] != null);
    // }
}
