C#
public partial class ProductDocument {
    public void OnChangeUrl() {
        if (this.Url != null && !this.Url.StartsWith("https://") && !this.Url.StartsWith("http://") && !this.Url.StartsWith("ftp://")) {
            throw new Exception($"Please enter a valid URL.\nExample: https://www.odoo.com\n\nInvalid URL: {this.Url}");
        }
    }

    public ProductDocument Create(Dictionary<string, object> values) {
        return Env.Create<ProductDocument>(values);
    }

    public List<ProductDocument> Create(List<Dictionary<string, object>> values) {
        return Env.Create<ProductDocument>(values);
    }

    public List<Dictionary<string, object>> CopyData(Dictionary<string, object> defaultValues = null) {
        var valsList = new List<Dictionary<string, object>>();
        foreach (var document in this) {
            var vals = new Dictionary<string, object>();
            if (defaultValues != null) {
                vals = defaultValues.ToDictionary(x => x.Key, x => x.Value);
            }
            vals["IrAttachmentId"] = document.IrAttachmentId.WithContext(new Dictionary<string, object> { { "NoDocument", true }, { "DisableProductDocumentsCreation", true } }).Copy(defaultValues).Id;
            valsList.Add(vals);
        }
        return valsList;
    }

    public void Unlink() {
        Env.Delete(this);
        this.IrAttachmentId.Unlink();
    }
}
