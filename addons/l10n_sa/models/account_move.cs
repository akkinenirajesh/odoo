csharp
public partial class AccountMove
{
    public void ComputeShowDeliveryDate()
    {
        base.ComputeShowDeliveryDate();
        if (this.CountryCode == "SA")
        {
            this.ShowDeliveryDate = this.IsSaleDocument();
        }
    }

    public void ComputeQrCodeStr()
    {
        string GetQrEncoding(byte tag, string field)
        {
            byte[] fieldByteArray = System.Text.Encoding.UTF8.GetBytes(field);
            byte[] tagEncoding = new byte[] { tag };
            byte[] lengthEncoding = new byte[] { (byte)fieldByteArray.Length };
            return System.Text.Encoding.UTF8.GetString(tagEncoding.Concat(lengthEncoding).Concat(fieldByteArray).ToArray());
        }

        string qrCodeStr = "";
        if (this.L10nSaConfirmationDatetime != null && !string.IsNullOrEmpty(this.Company.Vat))
        {
            string sellerNameEnc = GetQrEncoding(1, this.Company.DisplayName);
            string companyVatEnc = GetQrEncoding(2, this.Company.Vat);
            string timeSa = this.L10nSaConfirmationDatetime.Value.ToUniversalTime()
                .AddHours(3) // Riyadh is UTC+3
                .ToString("yyyy-MM-ddTHH:mm:ssZ");
            string timestampEnc = GetQrEncoding(3, timeSa);
            string invoiceTotalEnc = GetQrEncoding(4, Math.Abs(this.AmountTotalSigned).ToString("F2"));
            string totalVatEnc = GetQrEncoding(5, Math.Abs(this.AmountTaxSigned).ToString("F2"));

            string strToEncode = sellerNameEnc + companyVatEnc + timestampEnc + invoiceTotalEnc + totalVatEnc;
            qrCodeStr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(strToEncode));
        }
        this.L10nSaQrCodeStr = qrCodeStr;
    }

    public void Post(bool soft = true)
    {
        base.Post(soft);
        if (this.CountryCode == "SA" && this.IsSaleDocument())
        {
            var vals = new Dictionary<string, object>
            {
                { "L10nSaConfirmationDatetime", DateTime.Now }
            };
            if (this.DeliveryDate == null)
            {
                vals["DeliveryDate"] = this.InvoiceDate;
            }
            this.Write(vals);
        }
    }
}
