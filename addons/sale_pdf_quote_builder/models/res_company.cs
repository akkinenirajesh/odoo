csharp
public partial class ResCompany {
    public void EnsureHeaderNotEncrypted() {
        if (this.SaleHeader != null) {
            // call _ensure_document_not_encrypted(base64.b64decode(company.sale_header)) in C#
        }
    }

    public void EnsureFooterNotEncrypted() {
        if (this.SaleFooter != null) {
            // call _ensure_document_not_encrypted(base64.b64decode(company.sale_footer)) in C#
        }
    }
}
