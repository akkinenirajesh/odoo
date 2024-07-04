csharp
public partial class AccountMove {
    public string GetInvoiceReferenceSeOcr2(string reference) {
        return reference + luhn.calc_check_digit(reference);
    }

    public string GetInvoiceReferenceSeOcr3(string reference) {
        reference = reference + str(len(reference) + 2)[:1];
        return reference + luhn.calc_check_digit(reference);
    }

    public string GetInvoiceReferenceSeOcr4(string reference) {
        int ocrLength = Env.Get<AccountJournal>().L10nSeInvoiceOcrLength;

        if (reference.Length + 1 > ocrLength) {
            throw new Exception($"OCR Reference Number length is greater than allowed. Allowed length in invoice journal setting is {ocrLength}.");
        }

        reference = reference.PadLeft(ocrLength - 1, '0');
        return reference + luhn.calc_check_digit(reference);
    }

    public string GetInvoiceReferenceSeOcr2Invoice() {
        return GetInvoiceReferenceSeOcr2(this.Id.ToString());
    }

    public string GetInvoiceReferenceSeOcr3Invoice() {
        return GetInvoiceReferenceSeOcr3(this.Id.ToString());
    }

    public string GetInvoiceReferenceSeOcr4Invoice() {
        return GetInvoiceReferenceSeOcr4(this.Id.ToString());
    }

    public string GetInvoiceReferenceSeOcr2Partner() {
        return GetInvoiceReferenceSeOcr2(this.PartnerId.Ref.IsDecimal() ? this.PartnerId.Ref.ToString() : this.PartnerId.Id.ToString());
    }

    public string GetInvoiceReferenceSeOcr3Partner() {
        return GetInvoiceReferenceSeOcr3(this.PartnerId.Ref.IsDecimal() ? this.PartnerId.Ref.ToString() : this.PartnerId.Id.ToString());
    }

    public string GetInvoiceReferenceSeOcr4Partner() {
        return GetInvoiceReferenceSeOcr4(this.PartnerId.Ref.IsDecimal() ? this.PartnerId.Ref.ToString() : this.PartnerId.Id.ToString());
    }

    public void OnChangePartnerId() {
        if (this.PartnerId != null && this.MoveType == "in_invoice" && this.PartnerId.L10nSeDefaultVendorPaymentRef != null) {
            this.PaymentReference = this.PartnerId.L10nSeDefaultVendorPaymentRef;
        }
    }

    public void CheckPaymentReference() {
        if ((this.PaymentReference != null || this.State == "posted") && this.PartnerId != null && this.MoveType == "in_invoice" && this.PartnerId.L10nSeCheckVendorOcr && this.CountryCode == "SE") {
            try {
                luhn.validate(this.PaymentReference);
            } catch (Exception) {
                throw new Exception("Vendor require OCR Number as payment reference. Payment reference isn't a valid OCR Number.");
            }
        }
    }
}
