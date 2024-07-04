C#
public partial class AccountJournal {
    public void CheckL10nSeInvoiceOcrLength() {
        if (this.L10nSeInvoiceOcrLength < 6) {
            throw new Exception("OCR Reference Number length need to be greater than 5. Please correct settings under invoice journal settings.");
        }
    }
}
