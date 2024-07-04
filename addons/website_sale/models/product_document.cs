csharp
public partial class Website_ProductDocument {
    public void _UnsupportedProductProductDocumentOnEcommerce() {
        if (this.ResModel == "product.product" && this.ShownOnProductPage) {
            throw new ValidationError(_("Documents shown on product page cannot be restricted to a specific variant"));
        }
    }
}
