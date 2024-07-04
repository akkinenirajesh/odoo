csharp
public partial class SaleOrderTemplate {
    public void EnsureHeaderEncryption() {
        if (this.SaleHeader != null) {
            utils.EnsureDocumentNotEncrypted(Convert.FromBase64String(this.SaleHeader));
        }
    }

    public void EnsureFooterEncryption() {
        if (this.SaleFooter != null) {
            utils.EnsureDocumentNotEncrypted(Convert.FromBase64String(this.SaleFooter));
        }
    }
}
